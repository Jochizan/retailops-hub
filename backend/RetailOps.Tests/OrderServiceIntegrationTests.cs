using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Exceptions;
using RetailOps.Application.DTOs;
using RetailOps.Application.Services;
using RetailOps.Domain.Entities;
using RetailOps.Domain.Enums;
using RetailOps.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RetailOps.Tests;

public class TestDbContext : RetailOpsDbContext
{
    public TestDbContext(DbContextOptions<RetailOpsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var checkConstraint in entity.GetCheckConstraints().ToList())
            {
                entity.RemoveCheckConstraint(checkConstraint.Name!);
            }
        }

        modelBuilder.Entity<Sku>(entity => {
            entity.Property(e => e.AttributesJson).HasColumnType("TEXT");
        });

        modelBuilder.Entity<Inventory>(entity => {
            entity.Property(e => e.RowVersion).IsRequired(false);
        });
    }
}

public class OrderServiceIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestDbContext _context;
    private readonly OrderService _service;

    public OrderServiceIntegrationTests()
    {
        // Unique connection per test to avoid collisions
        _connection = new SqliteConnection($"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RetailOpsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
        
        _service = new OrderService(_context);
    }

    [Fact]
    public async Task CreateOrder_ValidStock_ShouldSucceed()
    {
        // Arrange
        var store = new Store { Name = "Test Store", Code = Guid.NewGuid().ToString() };
        var product = new Product { Name = "Test Product", Brand = "B1", Category = "C1", Description = "D1" };
        _context.Stores.Add(store);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var sku = new Sku { ProductId = product.Id, Code = "SKU-1", Price = 10, AttributesJson = "{}" };
        _context.Skus.Add(sku);
        await _context.SaveChangesAsync();

        _context.Inventory.Add(new Inventory { StoreId = store.Id, SkuId = sku.Id, OnHand = 10, Reserved = 0, ReorderPoint = 1 });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            StoreId = store.Id,
            Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { SkuId = sku.Id, Quantity = 3 } }
        };

        // Act
        var response = await _service.CreateOrderAsync(request);

        // Assert
        response.Should().NotBeNull();
        var inv = await _context.Inventory.FirstAsync(i => i.SkuId == sku.Id);
        inv.Reserved.Should().Be(3);
    }

    [Fact]
    public async Task ConfirmOrder_ShouldDecreaseOnHandAndReserved()
    {
        // Arrange
        var store = new Store { Name = "Test Store", Code = Guid.NewGuid().ToString() };
        var product = new Product { Name = "Test Product", Brand = "B1", Category = "C1", Description = "D1" };
        _context.Stores.Add(store);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var sku = new Sku { ProductId = product.Id, Code = "CONFIRM-1", Price = 10, AttributesJson = "{}" };
        _context.Skus.Add(sku);
        await _context.SaveChangesAsync();

        var inv = new Inventory { StoreId = store.Id, SkuId = sku.Id, OnHand = 10, Reserved = 2, ReorderPoint = 1 };
        _context.Inventory.Add(inv);
        await _context.SaveChangesAsync();

        var order = new Order { StoreId = store.Id, Status = OrderStatus.Reserved, CreatedAt = DateTime.UtcNow };
        order.Items.Add(new OrderItem { SkuId = sku.Id, Quantity = 2, UnitPrice = 10, SubTotal = 20 });
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        await _service.ConfirmOrderAsync(order.Id);

        // Assert
        var updatedInv = await _context.Inventory.FirstAsync(i => i.SkuId == sku.Id);
        updatedInv.OnHand.Should().Be(8);
        updatedInv.Reserved.Should().Be(0);
    }

    [Fact]
    public async Task CancelOrder_ShouldReleaseReservedStock()
    {
        // Arrange
        var store = new Store { Name = "Test Store", Code = Guid.NewGuid().ToString() };
        var product = new Product { Name = "Test Product", Brand = "B1", Category = "C1", Description = "D1" };
        _context.Stores.Add(store);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var sku = new Sku { ProductId = product.Id, Code = "CANCEL-1", Price = 10, AttributesJson = "{}" };
        _context.Skus.Add(sku);
        await _context.SaveChangesAsync();

        var inv = new Inventory { StoreId = store.Id, SkuId = sku.Id, OnHand = 10, Reserved = 5, ReorderPoint = 1 };
        _context.Inventory.Add(inv);
        await _context.SaveChangesAsync();

        var order = new Order { StoreId = store.Id, Status = OrderStatus.Reserved, CreatedAt = DateTime.UtcNow };
        order.Items.Add(new OrderItem { SkuId = sku.Id, Quantity = 5, UnitPrice = 10, SubTotal = 50 });
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        await _service.CancelOrderAsync(order.Id);

        // Assert
        var updatedInv = await _context.Inventory.FirstAsync(i => i.SkuId == sku.Id);
        updatedInv.Reserved.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}