using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetailOps.Application.Common.Interfaces;
using RetailOps.Domain.Entities;
using System.Text.Json;

namespace RetailOps.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that polls the OutboxEvents table and processes pending events.
/// This ensures "at-least-once" delivery of integration events.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceScopeFactory scopeFactory, ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing outbox events.");
            }

            // Poll interval: every 5 seconds for more responsiveness in Demo
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Outbox Worker is stopping.");
    }

    private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IRetailOpsDbContext>();

        // 1. Fetch top 20 pending events
        var events = await context.OutboxEvents
            .Where(e => e.ProcessedOn == null)
            .OrderBy(e => e.OccurredOn)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (!events.Any()) return;

        _logger.LogInformation("Processing {Count} outbox events.", events.Count);

        foreach (var outboxEvent in events)
        {
            try
            {
                // 2. Route by event type
                switch (outboxEvent.Type)
                {
                    case "OrderCreated":
                        // Simular envío a bus externo o email
                        _logger.LogInformation("Integration: Order {Payload} published.", outboxEvent.PayloadJson);
                        break;

                    case "StockLow":
                        await HandleStockLowAsync(context, outboxEvent.PayloadJson, stoppingToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown event type: {Type}", outboxEvent.Type);
                        break;
                }

                // 3. Mark as processed
                outboxEvent.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox event {Id}.", outboxEvent.Id);
                outboxEvent.Error = ex.Message;
            }
        }

        // 4. Save progress
        await context.SaveChangesAsync(stoppingToken);
    }

    private async Task HandleStockLowAsync(IRetailOpsDbContext context, string payloadJson, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payloadJson);
        int storeId = data.GetProperty("StoreId").GetInt32();
        int skuId = data.GetProperty("SkuId").GetInt32();
        int available = data.GetProperty("Available").GetInt32();
        int reorderPoint = data.GetProperty("ReorderPoint").GetInt32();

        // Business Rule: Only create alert if there isn't an 'open' or 'acknowledged' one for this SKU/Store
        var existingAlert = await context.StockAlerts
            .AnyAsync(a => a.StoreId == storeId && a.SkuId == skuId && a.Status != "resolved", ct);

        if (!existingAlert)
        {
            var alert = new StockAlert
            {
                StoreId = storeId,
                SkuId = skuId,
                Type = "stock_low",
                Status = "open",
                Message = $"Critical Stock: Only {available} units left (Reorder point: {reorderPoint})",
                CreatedAt = DateTime.UtcNow
            };
            context.StockAlerts.Add(alert);
            _logger.LogWarning("Created Alert: {Msg} for Sku {SkuId} in Store {StoreId}", alert.Message, skuId, storeId);
        }
    }
}
