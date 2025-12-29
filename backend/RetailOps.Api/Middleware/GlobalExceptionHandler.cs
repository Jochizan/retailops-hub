using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Application.Common.Exceptions;
using System.Net;

namespace RetailOps.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Detail = exception.Message
        };

        switch (exception)
        {
            case InsufficientStockException:
                problemDetails.Title = "Insufficient Stock";
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Extensions["errorCode"] = "INSUFFICIENT_STOCK";
                break;

            case ConcurrencyConflictException:
                problemDetails.Title = "Concurrency Conflict";
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Extensions["errorCode"] = "CONCURRENCY_CONFLICT";
                break;

            case InvalidOperationException:
                problemDetails.Title = "Invalid Operation";
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Extensions["errorCode"] = "INVALID_STATE_TRANSITION";
                break;

            case KeyNotFoundException:
                problemDetails.Title = "Resource Not Found";
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Extensions["errorCode"] = "NOT_FOUND";
                break;

            default:
                problemDetails.Title = "Server Error";
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Extensions["errorCode"] = "INTERNAL_SERVER_ERROR";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
