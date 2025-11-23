using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using NetGPT.API.Hubs;
using NetGPT.Application.Handlers;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Mappings;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Persistence.Repositories;
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
builder.Services.AddSingleton<IConversationMapper, ConversationMapper>();

// Agent Framework
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Register Tool Plugins at Runtime (Flexible DI)
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
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
