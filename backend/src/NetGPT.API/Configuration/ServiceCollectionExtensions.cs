// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetGPT.Application.Behaviors;
using NetGPT.Application.Handlers;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Services;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Declarative;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Persistence.Repositories;
using NetGPT.Infrastructure.Services;
using NetGPT.Infrastructure.Tools;

namespace NetGPT.API.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetGptServices(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"]
                ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");

            _ = services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            _ = services.AddDbContext<DefinitionDbContext>(options =>
                options.UseNpgsql(connectionString));

            _ = services.AddScoped<IDefinitionRepository, DefinitionRepository>();
            _ = services.AddSingleton<DeclarativeCache>();
            _ = services.AddScoped<IDeclarativeLoader, DeclarativeLoader>();

            _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateConversationHandler>());
            _ = services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            _ = services.AddScoped<IConversationRepository, ConversationRepository>();
            _ = services.AddScoped<IUnitOfWork, UnitOfWork>();

            _ = services.AddScoped<ITokenService, TokenService>();
            _ = services.AddScoped<RefreshTokenRepository>();
            _ = services.AddScoped<IUserRepository, UserRepository>();
            _ = services.AddSingleton<IPasswordHasher, PasswordHasher>();
            _ = services.AddSingleton<IConversationMapper, ConversationMapper>();

            _ = services.AddSingleton<IToolRegistry, ToolRegistry>();
            _ = services.AddScoped<IAgentFactory, AgentFactory>();
            _ = services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
            _ = services.AddSingleton<IOpenAIClientFactory, OpenAIClientFactory>();

            return services;
        }
    }
}
