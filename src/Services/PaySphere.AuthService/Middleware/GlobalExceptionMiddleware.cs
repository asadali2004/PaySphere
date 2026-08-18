using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.BuildingBlocks.Responses;

// Centralized exception handling middleware for the AuthService. It converts
// exceptions thrown by downstream middleware and controllers into consistent
// ApiResponse objects and appropriate HTTP status codes.
namespace PaySphere.AuthService.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions, logs them, and returns a
/// standardized JSON error response. Business exceptions deriving from
/// <see cref="PaySphere.BuildingBlocks.Exceptions.BaseException"/> are treated
/// as client errors (400 Bad Request).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware pipeline and intercepts exceptions to produce a
    /// standardized JSON error response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred.";

        if (exception is BaseException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = new List<string>
            {
                exception.Message
            }
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}
