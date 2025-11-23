#!/bin/bash

# Missing Application Interfaces
cat > src/NetGPT.Application/Interfaces/IAgentOrchestrator.cs << 'EOF'
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Agents;

namespace NetGPT.Application.Interfaces;

public interface IAgentOrchestrator
{
    Task<Result<AgentResponse>> ExecuteAsync(
        Conversation conversation,
        string userMessage,
        CancellationToken cancellationToken);
}
EOF

# Global Exception Middleware
cat > src/NetGPT.API/Middleware/GlobalExceptionMiddleware.cs << 'EOF'
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
EOF

# Swagger Configuration
cat > src/NetGPT.API/Configuration/SwaggerConfiguration.cs << 'EOF'
using Microsoft.OpenApi.Models;

namespace NetGPT.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NetGPT API",
                Version = "v1",
                Description = "ChatGPT clone with Agent Framework",
                Contact = new OpenApiContact
                {
                    Name = "NetGPT Team"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
EOF

# Update Program.cs to include middleware and mapper
cat > src/NetGPT.API/Program.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using NetGPT.API.Configuration;
using NetGPT.API.Hubs;
using NetGPT.API.Middleware;
using NetGPT.Application.Handlers;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Repositories;
using NetGPT.Infrastructure.Tools;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationHandler>());

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Mappers
builder.Services.AddScoped<IConversationMapper, ConversationMapper>();

// Agent Framework
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Register Tool Plugins at Runtime
builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IToolRegistry>();
    
    // Web Search Tool
    var webSearchTools = AIFunctionFactory.Create(new WebSearchToolPlugin());
    foreach (var tool in webSearchTools)
    {
        registry.RegisterTool(tool);
    }
    
    // Code Execution Tools
    var codeTools = AIFunctionFactory.Create(new CodeExecutionToolPlugin());
    foreach (var tool in codeTools)
    {
        registry.RegisterTool(tool);
    }
    
    // File Processing Tools
    var fileTools = AIFunctionFactory.Create(new FileProcessingToolPlugin());
    foreach (var tool in fileTools)
    {
        registry.RegisterTool(tool);
    }
    
    return registry;
});

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();
EOF

# Create initial migration script
cat > create_migration.sh << 'EOF'
#!/bin/bash
cd src/NetGPT.API
dotnet ef migrations add InitialCreate --project ../NetGPT.Infrastructure --startup-project .
dotnet ef database update --project ../NetGPT.Infrastructure --startup-project .
EOF
chmod +x create_migration.sh

# Create Messages Controller
cat > src/NetGPT.API/Controllers/MessagesController.cs << 'EOF'
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.Queries;

namespace NetGPT.API.Controllers;

[ApiController]
[Route("api/v1/conversations/{conversationId}/messages")]
public sealed class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetMessagesQuery(conversationId, userId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Message });
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
EOF

# Create build and run scripts
cat > build.sh << 'EOF'
#!/bin/bash
dotnet build NetGPT.sln
EOF
chmod +x build.sh

cat > run.sh << 'EOF'
#!/bin/bash
cd src/NetGPT.API
dotnet run
EOF
chmod +x run.sh

echo "All remaining files created successfully"
