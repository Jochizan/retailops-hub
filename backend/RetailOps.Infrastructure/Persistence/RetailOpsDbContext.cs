using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RetailOps.Domain.Entities;
using RetailOps.Application.Common.Interfaces;

namespace RetailOps.Infrastructure.Persistence
{
    public class RetailOpsDbContext : DbContext, IRetailOpsDbContext
    {
        public RetailOpsDbContext(DbContextOptions<RetailOpsDbContext> options) : base(options)
        {
        }

        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<AttributeType> AttributeTypes { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await Database.BeginTransactionAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // InventoryMovement
            modelBuilder.Entity<InventoryMovement>(entity =>
            {
                entity.ToTable("InventoryMovements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.StoreId, e.SkuId, e.CreatedAt }); // For Kardex query
                
                entity.HasOne(e => e.Store).WithMany().HasForeignKey(e => e.StoreId);
                entity.HasOne(e => e.Sku).WithMany().HasForeignKey(e => e.SkuId);
            });

            // StockAlert
            modelBuilder.Entity<StockAlert>(entity =>
            {
                entity.ToTable("StockAlerts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                // Filtered index to prevent duplicate 'open' alerts for the same SKU/Store
                entity.HasIndex(e => new { e.StoreId, e.SkuId, e.Type, e.Status })
                      .IsUnique()
                      .HasFilter("[Status] = 'open'");

                entity.HasOne(e => e.Store).WithMany().HasForeignKey(e => e.StoreId);
                entity.HasOne(e => e.Sku).WithMany().HasForeignKey(e => e.SkuId);
            });

            // AttributeType
            modelBuilder.Entity<AttributeType>(entity =>
            {
                entity.ToTable("AttributeTypes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Scope).IsRequired().HasMaxLength(10);
                
                entity.ToTable(t => t.HasCheckConstraint("CK_AttributeTypes_Scope", "Scope IN ('PRODUCT','SKUJSON')"));

                entity.HasData(
                    new AttributeType { Id = 1, Code = "FABRICANTE", Name = "Fabricante", Scope = "SKUJSON" },
                    new AttributeType { Id = 2, Code = "MARCA", Name = "Marca", Scope = "PRODUCT" },
                    new AttributeType { Id = 3, Code = "CONTENIDO", Name = "Contenido", Scope = "SKUJSON" }
                );
            });

            // Store
            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Stores");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();

                entity.HasData(
                    new Store { Id = 1, Name = "Tienda Central", Code = "STR001" },
                    new Store { Id = 2, Name = "Sucursal Norte", Code = "STR002" }
                );
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(100);

                entity.HasData(
                    new Product { Id = 1, Name = "Smartphone X1", Brand = "TechBrand", Category = "Electronics", Description = "High-end smartphone" },
                    new Product { Id = 2, Name = "Laptop Pro 15", Brand = "TechBrand", Category = "Electronics", Description = "Professional laptop" },
                    new Product { Id = 3, Name = "Coffee Maker", Brand = "HomePlus", Category = "Appliances", Description = "Automatic coffee maker" },
                    new Product { Id = 4, Name = "Wireless Mouse", Brand = "PeripheralsInc", Category = "Accessories", Description = "Ergonomic mouse" },
                    new Product { Id = 5, Name = "Organic Green Tea", Brand = "NatureLife", Category = "Groceries", Description = "Premium green tea" }
                );
            });

            // Sku
            modelBuilder.Entity<Sku>(entity =>
            {
                entity.ToTable("Skus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.AttributesJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Price).HasPrecision(18, 2);
                
                // SQL Server ISJSON check constraint
                entity.ToTable(t => t.HasCheckConstraint("CK_Skus_AttributesJson_IsJson", "ISJSON(AttributesJson) = 1"));

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Skus)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasData(
                    new Sku { Id = 1, ProductId = 1, Code = "SMX1-BLK", Price = 799.99m, AttributesJson = "{\"FABRICANTE\":\"TechFactory\", \"CONTENIDO\":\"Black Edition\"}" },
                    new Sku { Id = 2, ProductId = 1, Code = "SMX1-WHT", Price = 799.99m, AttributesJson = "{\"FABRICANTE\":\"TechFactory\", \"CONTENIDO\":\"White Edition\"}" },
                    new Sku { Id = 3, ProductId = 2, Code = "LP15-16GB", Price = 1299.00m, AttributesJson = "{\"FABRICANTE\":\"AssemblyCo\", \"CONTENIDO\":\"16GB RAM\"}" },
                    new Sku { Id = 4, ProductId = 2, Code = "LP15-32GB", Price = 1599.00m, AttributesJson = "{\"FABRICANTE\":\"AssemblyCo\", \"CONTENIDO\":\"32GB RAM\"}" },
                    new Sku { Id = 5, ProductId = 3, Code = "CM-AUTO", Price = 89.50m, AttributesJson = "{\"FABRICANTE\":\"HomeBuild\", \"CONTENIDO\":\"Standard\"}" },
                    new Sku { Id = 6, ProductId = 4, Code = "WM-SILENT", Price = 25.00m, AttributesJson = "{\"FABRICANTE\":\"LogiParts\", \"CONTENIDO\":\"Silent Clicks\"}" },
                    new Sku { Id = 7, ProductId = 5, Code = "GT-ORG-50", Price = 12.00m, AttributesJson = "{\"FABRICANTE\":\"EarthHarvest\", \"CONTENIDO\":\"50 Bags\"}" }
                );
            });

            // Inventory
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasKey(e => e.Id);
                
                // Unique (Store + Sku)
                entity.HasIndex(e => new { e.StoreId, e.SkuId }).IsUnique();

                // Concurrency token (RowVersion/Timestamp in SQL Server)
                entity.Property(e => e.RowVersion).IsRowVersion();

                // Check constraints
                entity.ToTable(t => 
                {
                    t.HasCheckConstraint("CK_Inventory_OnHand_NonNegative", "OnHand >= 0");
                    t.HasCheckConstraint("CK_Inventory_Reserved_NonNegative", "Reserved >= 0");
                    t.HasCheckConstraint("CK_Inventory_Available_NonNegative", "(OnHand - Reserved) >= 0");
                });

                entity.HasData(
                    // Store 1 (Tienda Central)
                    new Inventory { Id = 1, StoreId = 1, SkuId = 1, OnHand = 10, Reserved = 0, ReorderPoint = 5 },
                    new Inventory { Id = 2, StoreId = 1, SkuId = 2, OnHand = 5, Reserved = 0, ReorderPoint = 3 },
                    new Inventory { Id = 3, StoreId = 1, SkuId = 3, OnHand = 3, Reserved = 0, ReorderPoint = 5 }, // Low Stock
                    new Inventory { Id = 4, StoreId = 1, SkuId = 4, OnHand = 2, Reserved = 0, ReorderPoint = 2 }, // Low Stock
                    new Inventory { Id = 5, StoreId = 1, SkuId = 5, OnHand = 50, Reserved = 0, ReorderPoint = 10 },
                    new Inventory { Id = 6, StoreId = 1, SkuId = 6, OnHand = 100, Reserved = 0, ReorderPoint = 20 },
                    new Inventory { Id = 7, StoreId = 1, SkuId = 7, OnHand = 20, Reserved = 0, ReorderPoint = 10 },

                    // Store 2 (Sucursal Norte) - Different stock levels for demo
                    new Inventory { Id = 8, StoreId = 2, SkuId = 1, OnHand = 0, Reserved = 0, ReorderPoint = 5 }, // Out of Stock
                    new Inventory { Id = 9, StoreId = 2, SkuId = 2, OnHand = 20, Reserved = 0, ReorderPoint = 3 },
                    new Inventory { Id = 10, StoreId = 2, SkuId = 3, OnHand = 8, Reserved = 0, ReorderPoint = 5 },
                    new Inventory { Id = 11, StoreId = 2, SkuId = 5, OnHand = 15, Reserved = 0, ReorderPoint = 10 }
                );
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>(); // Store enum as string for readability
                entity.HasIndex(e => new { e.StoreId, e.Status }); // Common query pattern
                
                entity.HasOne(e => e.Store)
                      .WithMany()
                      .HasForeignKey(e => e.StoreId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sku)
                      .WithMany()
                      .HasForeignKey(e => e.SkuId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // IdempotencyKey
            modelBuilder.Entity<IdempotencyKey>(entity =>
            {
                entity.ToTable("IdempotencyKeys");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Method).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Path).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RequestHash).IsRequired().HasMaxLength(64); // SHA256 string length
            });

            // AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            });

            // OutboxEvent
            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.ToTable("OutboxEvents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            });
        }
    }
}