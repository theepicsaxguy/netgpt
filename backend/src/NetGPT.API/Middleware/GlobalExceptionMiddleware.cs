using System.Net;
using System.Text.Json;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Exceptions;

namespace NetGPT.API.Middleware;

public sealed class GlobalExceptionMiddleware
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
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorResponse) = exception switch
        {
            ConversationNotFoundException => (HttpStatusCode.NotFound, new ErrorResponse("NotFound", exception.Message, null)),
            UnauthorizedConversationAccessException => (HttpStatusCode.Forbidden, new ErrorResponse("Forbidden", exception.Message, null)),
            DomainException => (HttpStatusCode.BadRequest, new ErrorResponse("BadRequest", exception.Message, null)),
            _ => (HttpStatusCode.InternalServerError, new ErrorResponse("InternalError", "An error occurred processing your request", null))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
