// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers
{
    public class CreateConversationHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateConversationHandler> logger) : IRequestHandler<CreateConversationCommand, Result<ConversationResponse>>
    {
        private static readonly Action<ILogger, Guid, Exception?> CreateConversationStart = LoggerMessage.Define<Guid>(
            LogLevel.Debug,
            new EventId(1, "CreateConversationStart"),
            "CreateConversationHandler.Handle start for user {UserId}");

        private static readonly Action<ILogger, Exception?> ConversationAdded = LoggerMessage.Define(
            LogLevel.Debug,
            new EventId(2, "ConversationAdded"),
            "Conversation added to repository, saving changes...");

        private static readonly Action<ILogger, Guid, Exception?> SaveChangesCompleted = LoggerMessage.Define<Guid>(
            LogLevel.Debug,
            new EventId(3, "SaveChangesCompleted"),
            "SaveChangesAsync completed for conversation {ConversationId}");

        private readonly IConversationRepository repository = repository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        private readonly ILogger<CreateConversationHandler> logger = logger;

        public async Task<Result<ConversationResponse>> Handle(
            CreateConversationCommand request,
            CancellationToken cancellationToken)
        {
            CreateConversationStart(logger, request.UserId, null);
            UserId userId = UserId.From(request.UserId);
            AgentConfiguration agentConfig = MapToAgentConfiguration(request.Configuration);
            Conversation conversation = Conversation.Create(userId, request.Title, agentConfig);

            _ = await repository.AddAsync(conversation, cancellationToken);
            ConversationAdded(logger, null);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken);
            SaveChangesCompleted(logger, conversation.Id.Value, null);

            ConversationResponse response = new(
                conversation.Id.Value,
                conversation.Title,
                conversation.CreatedAt,
                conversation.UpdatedAt,
                0);

            return Result.Success(response);
        }

        private static AgentConfiguration MapToAgentConfiguration(AgentConfigurationDto? dto)
        {
            if (dto is null)
            {
                return AgentConfiguration.Default();
            }

            List<AgentDefinition>? agents = dto.Agents?.Select(a => new AgentDefinition(
                a.Name,
                a.Instructions,
                a.ModelName ?? "gpt-4o",
                a.Temperature ?? 0.7f,
                a.MaxTokens ?? 4000)).ToList();

            return new AgentConfiguration(
                dto.ModelName ?? "gpt-4o",
                dto.Temperature ?? 0.7f,
                dto.MaxTokens ?? 4000,
                null, // TopP etc not in DTO
                null,
                null,
                agents);
        }
    }
}
