using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Exceptions;
using RetailOps.Application.Common.Interfaces;
using RetailOps.Application.DTOs;
using RetailOps.Application.Interfaces;
using RetailOps.Domain.Entities;
using RetailOps.Domain.Enums;
using System.Text.Json;

namespace RetailOps.Application.Services;

/// <summary>
/// Service responsible for handling Order lifecycle and related Inventory reservations.
/// Implements the "Saga" of creating an order by ensuring consistency between Order and Inventory aggregates.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRetailOpsDbContext _context;

    public OrderService(IRetailOpsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new order and reserves stock atomically.
    /// </summary>
    /// <param name="request">The order creation request containing items and store context.</param>
    /// <returns>A response containing the created Order ID and status.</returns>
    /// <exception cref="InsufficientStockException">Thrown if any item does not have enough available stock (OnHand - Reserved).</exception>
    /// <exception cref="ConcurrencyConflictException">Thrown if the stock was modified by another process during the transaction.</exception>
    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // 1. Basic validation (Fail fast)
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item.");

        if (request.Items.Any(i => i.Quantity <= 0))
            throw new ArgumentException("Item quantity must be greater than zero.");

        var storeExists = await _context.Stores.AnyAsync(s => s.Id == request.StoreId);
        if (!storeExists)
            throw new KeyNotFoundException($"Store {request.StoreId} does not exist.");

        // 2. Start Explicit Transaction
        using var transaction = await _context.BeginTransactionAsync();

        try
        {
            // 3. Prepare the Order entity
            var order = new Order
            {
                StoreId = request.StoreId,
                Status = OrderStatus.Reserved,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // 4. Process each line item
            foreach (var itemDto in request.Items)
            {
                // 4.1. Load Inventory
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.StoreId == request.StoreId && i.SkuId == itemDto.SkuId);

                if (inventory == null)
                {
                    throw new InsufficientStockException($"No inventory record found for SKU {itemDto.SkuId} in Store {request.StoreId}.");
                }

                // 4.2. Business Logic: Check Availability
                var available = inventory.OnHand - inventory.Reserved;
                if (available < itemDto.Quantity)
                {
                    throw new InsufficientStockException($"Insufficient stock for SKU {itemDto.SkuId}. Requested: {itemDto.Quantity}, Available: {available}.");
                }

                // 4.3. Update State (Reserve Stock)
                inventory.Reserved += itemDto.Quantity;

                // 4.4. Fetch Pricing
                var sku = await _context.Skus.FindAsync(itemDto.SkuId);
                var unitPrice = sku?.Price ?? 0;

                // 4.5. Add Line Item
                var orderItem = new OrderItem
                {
                    SkuId = itemDto.SkuId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice,
                    SubTotal = unitPrice * itemDto.Quantity
                };

                order.Items.Add(orderItem);
                totalAmount += orderItem.SubTotal;

                // Log Movement: Reserve
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    StoreId = request.StoreId,
                    SkuId = itemDto.SkuId,
                    Type = "reserve",
                    Quantity = itemDto.Quantity,
                    Reference = $"Order Create",
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.TotalAmount = totalAmount;

            // 5. Add Order to Context
            _context.Orders.Add(order);

            // 6. First Save Changes (Persist Order & Inventory)
            await _context.SaveChangesAsync(CancellationToken.None);

            // 7. Outbox Pattern (S2-08)
            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = "OrderCreated",
                OccurredOn = DateTime.UtcNow,
                PayloadJson = JsonSerializer.Serialize(new 
                { 
                    OrderId = order.Id, 
                    StoreId = order.StoreId, 
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString()
                })
            };

            _context.OutboxEvents.Add(outboxEvent);

            // 7.1. Check for Low Stock (S3-05)
            foreach (var itemDto in request.Items)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.StoreId == request.StoreId && i.SkuId == itemDto.SkuId);
                
                if (inventory != null && (inventory.OnHand - inventory.Reserved) <= inventory.ReorderPoint)
                {
                    var stockLowEvent = new OutboxEvent
                    {
                        Id = Guid.NewGuid(),
                        Type = "StockLow",
                        OccurredOn = DateTime.UtcNow,
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            StoreId = inventory.StoreId,
                            SkuId = inventory.SkuId,
                            Available = inventory.OnHand - inventory.Reserved,
                            ReorderPoint = inventory.ReorderPoint
                        })
                    };
                    _context.OutboxEvents.Add(stockLowEvent);
                }
            }

            // 8. Second Save Changes (Persist Outbox Events)
            await _context.SaveChangesAsync(CancellationToken.None);

            // 9. Commit Transaction
            await transaction.CommitAsync();

            return new CreateOrderResponse
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new ConcurrencyConflictException("The stock was modified by another transaction. Please retry.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ConfirmOrderAsync(int id)
    {
        using var transaction = await _context.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new KeyNotFoundException($"Order {id} not found.");

            if (order.Status != OrderStatus.Reserved)
                throw new InvalidOperationException($"Cannot confirm order in state {order.Status}. Only Reserved orders can be confirmed.");

            foreach (var item in order.Items)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.StoreId == order.StoreId && i.SkuId == item.SkuId);

                if (inventory == null) throw new InvalidOperationException($"Inventory not found for SKU {item.SkuId}");

                inventory.Reserved -= item.Quantity;
                inventory.OnHand -= item.Quantity;

                // Log Movement: Confirm (Consume)
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    StoreId = order.StoreId,
                    SkuId = item.SkuId,
                    Type = "confirm",
                    Quantity = item.Quantity,
                    Reference = $"Order {order.Id}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = "OrderConfirmed",
                OccurredOn = DateTime.UtcNow,
                PayloadJson = JsonSerializer.Serialize(new { OrderId = order.Id })
            };
            _context.OutboxEvents.Add(outboxEvent);

            await _context.SaveChangesAsync(CancellationToken.None);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CancelOrderAsync(int id)
    {
        using var transaction = await _context.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new KeyNotFoundException($"Order {id} not found.");

            if (order.Status != OrderStatus.Reserved)
                throw new InvalidOperationException($"Cannot cancel order in state {order.Status}. Only Reserved orders can be cancelled.");

            foreach (var item in order.Items)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.StoreId == order.StoreId && i.SkuId == item.SkuId);

                if (inventory != null)
                {
                    inventory.Reserved -= item.Quantity;

                    // Log Movement: Cancel (Release)
                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        StoreId = order.StoreId,
                        SkuId = item.SkuId,
                        Type = "cancel",
                        Quantity = item.Quantity,
                        Reference = $"Order {order.Id}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = "OrderCancelled",
                OccurredOn = DateTime.UtcNow,
                PayloadJson = JsonSerializer.Serialize(new { OrderId = order.Id })
            };
            _context.OutboxEvents.Add(outboxEvent);

            await _context.SaveChangesAsync(CancellationToken.None);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Items)
            .ThenInclude(i => i.Sku)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return null;

        return MapToDto(order);
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int? storeId, string? status)
    {
        var query = _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Items)
            .ThenInclude(i => i.Sku)
            .AsNoTracking()
            .AsQueryable();

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var statusEnum))
        {
            query = query.Where(o => o.Status == statusEnum);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            StoreId = order.StoreId,
            StoreName = order.Store?.Name ?? "Unknown",
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                SkuId = i.SkuId,
                SkuCode = i.Sku?.Code ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }
}