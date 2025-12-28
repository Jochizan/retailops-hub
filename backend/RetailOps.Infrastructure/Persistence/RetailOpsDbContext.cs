using Microsoft.EntityFrameworkCore;
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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AttributeType
            modelBuilder.Entity<AttributeType>(entity =>
            {
                entity.ToTable("attribute_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Scope).IsRequired().HasMaxLength(10);
                
                entity.ToTable(t => t.HasCheckConstraint("CK_attribute_types_scope", "Scope IN ('PRODUCT','SKUJSON')"));

                entity.HasData(
                    new AttributeType { Id = 1, Code = "FABRICANTE", Name = "Fabricante", Scope = "SKUJSON" },
                    new AttributeType { Id = 2, Code = "MARCA", Name = "Marca", Scope = "PRODUCT" },
                    new AttributeType { Id = 3, Code = "CONTENIDO", Name = "Contenido", Scope = "SKUJSON" }
                );
            });

            // Store
            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(100);
            });

            // Sku
            modelBuilder.Entity<Sku>(entity =>
            {
                entity.ToTable("skus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.AttributesJson).HasColumnType("nvarchar(max)");
                
                // SQL Server ISJSON check constraint
                entity.ToTable(t => t.HasCheckConstraint("CK_skus_attributes_isjson", "ISJSON(AttributesJson) = 1"));

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Skus)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Inventory
            modelBuilder.Entity<Inventory>(entity =>
            {
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
            });

            // AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            });

            // OutboxEvent
            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            });
        }
    }
}
