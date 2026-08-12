using System.Net;
using System.Text.Json;
using PaySphere.BuildingBlocks.Exceptions;
using PaySphere.BuildingBlocks.Responses;
using PaySphere.WalletService.Exceptions;

namespace PaySphere.WalletService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";

        if (exception is WalletNotFoundException)
        {
            statusCode = HttpStatusCode.NotFound;
            message = exception.Message;
        }
        else if (exception is WalletAlreadyExistsException or WalletNotActiveException or BaseException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { exception.Message }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
