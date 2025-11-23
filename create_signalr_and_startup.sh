#!/bin/bash

# Create SignalR Hub for Streaming
cat > src/NetGPT.API/Hubs/ConversationHub.cs << 'EOF'
using Microsoft.AspNetCore.SignalR;
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Interfaces;

namespace NetGPT.API.Hubs;

public sealed class ConversationHub : Hub
{
    private readonly IMediator _mediator;
    private readonly IConversationRepository _repository;
    private readonly IAgentOrchestrator _orchestrator;

    public ConversationHub(
        IMediator mediator,
        IConversationRepository repository,
        IAgentOrchestrator orchestrator)
    {
        _mediator = mediator;
        _repository = repository;
        _orchestrator = orchestrator;
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = GetCurrentUserId();

        try
        {
            var conversation = await _repository.GetByIdAsync(conversationId);
            if (conversation == null || conversation.UserId != userId)
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found or unauthorized");
                return;
            }

            // Add user message
            var messageId = Guid.NewGuid();
            await Clients.Caller.SendAsync("MessageStarted", messageId);

            // Stream agent response
            await foreach (var chunk in StreamAgentResponse(conversation, content))
            {
                await Clients.Caller.SendAsync("MessageChunk", new
                {
                    MessageId = messageId,
                    Content = chunk.Content,
                    ToolInvocations = chunk.ToolInvocations,
                    IsComplete = chunk.IsComplete
                });
            }

            await Clients.Caller.SendAsync("MessageCompleted", messageId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    private async IAsyncEnumerable<StreamChunk> StreamAgentResponse(
        Domain.Aggregates.ConversationAggregate.Conversation conversation,
        string userMessage)
    {
        // This would integrate with actual Agent Framework streaming
        yield return new StreamChunk("Hello", new List<string>(), false);
        await Task.Delay(100);
        yield return new StreamChunk(" World", new List<string>(), true);
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}

public record StreamChunk(string Content, List<string> ToolInvocations, bool IsComplete);
EOF

# Create Program.cs with complete DI setup
cat > src/NetGPT.API/Program.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using NetGPT.API.Hubs;
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
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware Pipeline
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

# Create appsettings.json
cat > src/NetGPT.API/appsettings.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=netgpt;Username=postgres;Password=postgres"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here",
    "DefaultModel": "gpt-4o",
    "MaxTokens": 4000
  }
}
EOF

# Create appsettings.Development.json
cat > src/NetGPT.API/appsettings.Development.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}",
    "DefaultModel": "gpt-4o-mini",
    "MaxTokens": 2000
  }
}
EOF

echo "SignalR Hub and Startup configuration created"
