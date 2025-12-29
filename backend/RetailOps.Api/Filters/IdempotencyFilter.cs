using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RetailOps.Application.Common.Interfaces;
using RetailOps.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RetailOps.Api.Filters;

/// <summary>
/// Filter to handle Idempotency logic for POST requests.
/// Ensures that re-sending a request with the same 'Idempotency-Key' returns the same result
/// without executing the business logic twice.
/// </summary>
public class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IRetailOpsDbContext _context;
    private const string HeaderName = "Idempotency-Key";

    public IdempotencyFilter(IRetailOpsDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Only apply to POST and check for the header
        if (context.HttpContext.Request.Method != HttpMethods.Post)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
        {
            // If the header is missing, we decide if we want to enforce it or just proceed.
            // For Orders, it's safer to enforce it.
            context.Result = new BadRequestObjectResult(new { error = $"The header '{HeaderName}' is required for this operation." });
            return;
        }

        var path = context.HttpContext.Request.Path.ToString();
        var method = context.HttpContext.Request.Method;
        
        // 2. Calculate Request Hash
        var requestBody = await ReadBodyAsync(context.HttpContext.Request);
        var requestHash = CalculateHash(requestBody);

        // 3. Check if the key already exists
        var existingKey = await _context.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == idempotencyKey.ToString());

        if (existingKey != null)
        {
            // Case A: Hash matches -> Return the cached response
            if (existingKey.RequestHash == requestHash)
            {
                if (existingKey.ResponseJson == null)
                {
                    // This means the first request is still processing or crashed
                    context.Result = new ConflictObjectResult(new { error = "Request with this idempotency key is already in progress or failed to complete." });
                    return;
                }

                // Deserializing the cached response
                var cachedResult = JsonSerializer.Deserialize<object>(existingKey.ResponseJson);
                context.Result = new ObjectResult(cachedResult) { StatusCode = existingKey.StatusCode };
                return;
            }
            else
            {
                // Case B: Hash differs -> 409 Conflict (Key reuse with different data)
                context.Result = new ConflictObjectResult(new { error = "Idempotency-Key reuse conflict. The key was previously used with a different request body." });
                return;
            }
        }

        // 4. Create new Idempotency Key record (Locking phase)
        var newEntry = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = idempotencyKey.ToString(),
            Method = method,
            Path = path,
            RequestHash = requestHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.IdempotencyKeys.Add(newEntry);
        await _context.SaveChangesAsync(CancellationToken.None);

        // 5. Execute the actual action/controller
        var executedContext = await next();

        // 6. Capture and save the response
        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
        {
            newEntry.ResponseJson = JsonSerializer.Serialize(objectResult.Value);
            newEntry.StatusCode = objectResult.StatusCode;
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        else if (executedContext.Exception != null)
        {
            // If it failed with an exception, we might want to keep it as "null" 
            // so the user can retry with the SAME key (depending on business rule).
            // For now, we leave it null to allow retry after failure.
        }
    }

    private async Task<string> ReadBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0; // Reset position for the controller
        return body;
    }

    private string CalculateHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
