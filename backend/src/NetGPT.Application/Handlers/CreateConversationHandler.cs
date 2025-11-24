// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
        IUnitOfWork unitOfWork) : IRequestHandler<CreateConversationCommand, Result<ConversationResponse>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Result<ConversationResponse>> Handle(
            CreateConversationCommand request,
            CancellationToken cancellationToken)
        {
            UserId userId = UserId.From(request.UserId);
            AgentConfiguration agentConfig = MapToAgentConfiguration(request.Configuration);
            Conversation conversation = Conversation.Create(userId, request.Title, agentConfig);

            _ = await repository.AddAsync(conversation, cancellationToken);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken);

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

            var agents = dto.Agents?.Select(a => new AgentDefinition(
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
