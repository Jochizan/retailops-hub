namespace RetailOps.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        // Standard ASP.NET Core way to push into log scope
        var logger = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();
        using (logger.BeginScope("CorrelationId:{CorrelationId}", correlationId.ToString() ?? "unknown"))
        {
            await _next(context);
        }
    }
}