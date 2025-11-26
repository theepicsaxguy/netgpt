// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetGPT.API.Configuration;
using NetGPT.API.Hubs;
using NetGPT.Application.Handlers;
using NetGPT.Application.Behaviors;
using MediatR;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Services;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Persistence.Repositories;
using NetGPT.Infrastructure.Services;
using NetGPT.Infrastructure.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// If ASPNETCORE_ENVIRONMENT isn't explicitly set, auto-detect Development
// by checking for an `appsettings.Development.json` file in the content root.
// This allows `dotnet run` to pick up the Development environment without
// requiring the caller to export environment variables.
if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    string devSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Development.json");
    if (File.Exists(devSettingsPath))
    {
        builder.Environment.EnvironmentName = Environments.Development;
        Console.WriteLine("ASPNETCORE_ENVIRONMENT not set - defaulting to Development based on appsettings.Development.json");
    }
}

// Bind to URLs from configuration if present (no environment exports required).
IConfigurationSection kestrelSectionAll = builder.Configuration.GetSection("Kestrel:Endpoints");
string? httpUrlAll = kestrelSectionAll.GetSection("Http")?["Url"];
string? httpsUrlAll = kestrelSectionAll.GetSection("Https")?["Url"];

if (!string.IsNullOrWhiteSpace(httpUrlAll) && !string.IsNullOrWhiteSpace(httpsUrlAll))
{
    _ = builder.WebHost.UseUrls(httpUrlAll, httpsUrlAll);
}

// Configuration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Database
string connectionString = builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"]
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Declarative definitions DB context (use same connection string)
builder.Services.AddDbContext<NetGPT.Infrastructure.Persistence.DefinitionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Definition repository
builder.Services.AddScoped<NetGPT.Infrastructure.Declarative.IDefinitionRepository, NetGPT.Infrastructure.Declarative.DefinitionRepository>();

// Declarative loader and cache
builder.Services.AddSingleton<NetGPT.Infrastructure.Declarative.DeclarativeCache>();
builder.Services.AddScoped<NetGPT.Infrastructure.Declarative.IDeclarativeLoader, NetGPT.Infrastructure.Declarative.DeclarativeLoader>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationHandler>());

// MediatR pipeline behaviors (FluentValidation)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Token service for JWT and refresh tokens
builder.Services.AddScoped<ITokenService, TokenService>();
// Refresh token repository
builder.Services.AddScoped<NetGPT.Infrastructure.Persistence.Repositories.RefreshTokenRepository>();
// User repository
builder.Services.AddScoped<NetGPT.Application.Interfaces.IUserRepository, NetGPT.Infrastructure.Persistence.Repositories.UserRepository>();

// Simple password hasher
builder.Services.AddSingleton<NetGPT.Infrastructure.Services.IPasswordHasher, NetGPT.Infrastructure.Services.PasswordHasher>();

// Mappers
builder.Services.AddSingleton<IConversationMapper, ConversationMapper>();

// Agent Framework
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Declarative loader and cache
builder.Services.AddSingleton<NetGPT.Infrastructure.Declarative.DeclarativeCache>();
builder.Services.AddScoped<NetGPT.Infrastructure.Declarative.IDeclarativeLoader, NetGPT.Infrastructure.Declarative.DeclarativeLoader>();

// OpenAI client factory used by SDK-backed adapter
builder.Services.AddSingleton<NetGPT.Infrastructure.Agents.IOpenAIClientFactory, NetGPT.Infrastructure.Agents.OpenAIClientFactory>();

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

// Authentication is configured below (single registration block)

// Authorization placeholder: AdminOnly policy (implementation of roles/claims is out of scope)
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        _ = policy
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

if (!builder.Environment.IsDevelopment())
{
    _ = app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();
