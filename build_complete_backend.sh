#!/bin/bash
set -e

BASE_DIR="/home/claude/NetGPT.Backend"

#########################
# APPLICATION LAYER - Handlers
#########################

cat > src/NetGPT.Application/Handlers/CreateConversationHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public sealed class CreateConversationHandler : IRequestHandler<CreateConversationCommand, Result<ConversationResponse>>
{
    private the IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConversationMapper _mapper;

    public CreateConversationHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork,
        IConversationMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var configResult = AgentConfiguration.Create(
            request.Configuration?.ModelName ?? "gpt-4o",
            request.Configuration?.Temperature ?? 0.7f,
            request.Configuration?.MaxTokens ?? 4000,
            request.Configuration?.TopP ?? 1.0f,
            request.Configuration?.FrequencyPenalty ?? 0.0f,
            request.Configuration?.PresencePenalty ?? 0.0f);

        if (configResult.IsFailure)
        {
            return Result.Failure<ConversationResponse>(configResult.Error);
        }

        var conversationResult = Conversation.Create(
            request.UserId,
            request.Title ?? "New Conversation",
            configResult.Value);

        if (conversationResult.IsFailure)
        {
            return Result.Failure<ConversationResponse>(conversationResult.Error);
        }

        var conversation = await _repository.AddAsync(conversationResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.ToResponse(conversation);
    }
}
EOF

cat > src/NetGPT.Application/Handlers/SendMessageHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Exceptions;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Application.Handlers;

public sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<MessageResponse>>
{
    private readonly IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IConversationMapper _mapper;

    public SendMessageHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork,
        IAgentOrchestrator orchestrator,
        IConversationMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _orchestrator = orchestrator;
        _mapper = mapper;
    }

    public async Task<Result<MessageResponse>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<MessageResponse>(
                new Error("Conversation.NotFound", $"Conversation {request.ConversationId} not found"));
        }

        if (conversation.UserId != request.UserId)
        {
            return Result.Failure<MessageResponse>(
                new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        var attachments = request.Attachments?
            .Select(a => MessageAttachment.Create(a.FileName, a.FileUrl, a.ContentType, a.FileSizeBytes).Value)
            .ToList() ?? new List<MessageAttachment>();

        var contentResult = MessageContent.Create(request.Content, MessageRole.User, attachments);
        if (contentResult.IsFailure)
        {
            return Result.Failure<MessageResponse>(contentResult.Error);
        }

        var userMessage = conversation.AddMessage(contentResult.Value);
        if (userMessage.IsFailure)
        {
            return Result.Failure<MessageResponse>(userMessage.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var agentResponseResult = await _orchestrator.ExecuteAsync(
            conversation,
            request.Content,
            cancellationToken);

        if (agentResponseResult.IsFailure)
        {
            return Result.Failure<MessageResponse>(agentResponseResult.Error);
        }

        var assistantContentResult = MessageContent.Create(
            agentResponseResult.Value.Text,
            MessageRole.Assistant);

        if (assistantContentResult.IsFailure)
        {
            return Result.Failure<MessageResponse>(assistantContentResult.Error);
        }

        var assistantMessage = conversation.AddMessage(assistantContentResult.Value);
        if (assistantMessage.IsFailure)
        {
            return Result.Failure<MessageResponse>(assistantMessage.Error);
        }

        assistantMessage.Value.SetMetadata(MessageMetadata.Create(
            agentResponseResult.Value.TokensUsed,
            agentResponseResult.Value.ResponseTime,
            conversation.AgentConfiguration.ModelName,
            agentResponseResult.Value.ToolsInvoked));

        conversation.IncrementTokenUsage(agentResponseResult.Value.TokensUsed);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.ToMessageResponse(assistantMessage.Value);
    }
}
EOF

echo "Handlers created"
