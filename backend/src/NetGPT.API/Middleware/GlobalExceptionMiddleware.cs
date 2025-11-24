// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.API.Middleware
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Exceptions;

    public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        private readonly RequestDelegate next = next;
        private readonly ILogger<GlobalExceptionMiddleware> logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "An unhandled exception occurred");
                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            (HttpStatusCode statusCode, ErrorResponse? errorResponse) = exception switch
            {
                ConversationNotFoundException => (HttpStatusCode.NotFound, new ErrorResponse("NotFound", exception.Message, null)),
                UnauthorizedConversationAccessException => (HttpStatusCode.Forbidden, new ErrorResponse("Forbidden", exception.Message, null)),
                DomainException => (HttpStatusCode.BadRequest, new ErrorResponse("BadRequest", exception.Message, null)),
                _ => (HttpStatusCode.InternalServerError, new ErrorResponse("InternalError", "An error occurred processing your request", null)),
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
