// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using NetGPT.API.Configuration;
using NetGPT.API.Hubs;
using NetGPT.Application.Handlers;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Services;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Persistence.Repositories;
using NetGPT.Infrastructure.Tools;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Database
string connectionString = builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"]
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationHandler>());

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Mappers
builder.Services.AddSingleton<IConversationMapper, ConversationMapper>();

// Agent Framework
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Register Tool Plugins at Runtime (Flexible DI)
builder.Services.AddSingleton(sp =>
{
    IToolRegistry registry = sp.GetRequiredService<IToolRegistry>();

    // Web Search Tool
    AIFunction webSearchTool = AIFunctionFactory.Create(WebSearchToolPlugin.SearchWeb);
    registry.RegisterTool(webSearchTool);

    // Code Execution Tools
    AIFunction pythonTool = AIFunctionFactory.Create(CodeExecutionToolPlugin.ExecutePython);
    AIFunction jsTool = AIFunctionFactory.Create(CodeExecutionToolPlugin.ExecuteJavaScript);
    registry.RegisterTool(pythonTool);
    registry.RegisterTool(jsTool);

    // File Processing Tools
    AIFunction pdfTool = AIFunctionFactory.Create(FileProcessingToolPlugin.ExtractPdfText);
    AIFunction imageTool = AIFunctionFactory.Create(FileProcessingToolPlugin.AnalyzeImage);
    registry.RegisterTool(pdfTool);
    registry.RegisterTool(imageTool);

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
        _ = policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

WebApplication app = builder.Build();

// Ensure database is ready before starting
int maxRetries = 10;
int retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        using Npgsql.NpgsqlConnection connection = new(connectionString);
        connection.Open();
        Console.WriteLine("✓ Database connection successful!");
        connection.Close();
        break;
    }
    catch (Exception ex)
    {
        retryCount++;
        Console.WriteLine(
            $"✗ Database connection attempt {retryCount}/{maxRetries} failed: {ex.Message}");
        if (retryCount >= maxRetries)
        {
            throw new InvalidOperationException(
                "Failed to connect to the database after multiple attempts. " +
                "Ensure PostgreSQL is running and accessible.",
                ex);
        }

        System.Threading.Thread.Sleep(1000);
    }
}

// Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();
