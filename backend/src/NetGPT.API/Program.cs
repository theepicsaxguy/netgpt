// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
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
    WebSearchToolPlugin webSearchPlugin = new();
    AIFunction webSearchTool = AIFunctionFactory.Create(webSearchPlugin.SearchWeb);
    registry.RegisterTool(webSearchTool);

    // Code Execution Tools
    CodeExecutionToolPlugin codePlugin = new();
    AIFunction pythonTool = AIFunctionFactory.Create(codePlugin.ExecutePython);
    AIFunction jsTool = AIFunctionFactory.Create(codePlugin.ExecuteJavaScript);
    registry.RegisterTool(pythonTool);
    registry.RegisterTool(jsTool);

    // File Processing Tools
    FileProcessingToolPlugin filePlugin = new();
    AIFunction pdfTool = AIFunctionFactory.Create(filePlugin.ExtractPdfText);
    AIFunction imageTool = AIFunctionFactory.Create(filePlugin.AnalyzeImage);
    registry.RegisterTool(pdfTool);
    registry.RegisterTool(imageTool);

    return registry;
});

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "NetGPT API", Version = "v1" });
});

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

// Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();
