#!/bin/bash

# Application Interfaces
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

cat > src/NetGPT.Application/Interfaces/IConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Application.Interfaces;

public interface IConversationMapper
{
    ConversationResponse ToResponse(Conversation conversation);
    MessageResponse ToMessageResponse(Message message);
}
EOF

# Mapper Implementation
cat > src/NetGPT.Application/Mappings/ConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Application.Mappings;

public sealed class ConversationMapper : IConversationMapper
{
    public ConversationResponse ToResponse(Conversation conversation)
    {
        return new ConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.Status.ToString(),
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages.Count,
            conversation.TokensUsed,
            new AgentConfigurationDto(
                conversation.AgentConfiguration.ModelName,
                conversation.AgentConfiguration.Temperature,
                conversation.AgentConfiguration.MaxTokens,
                conversation.AgentConfiguration.TopP,
                conversation.AgentConfiguration.FrequencyPenalty,
                conversation.AgentConfiguration.PresencePenalty));
    }

    public MessageResponse ToMessageResponse(Message message)
    {
        return new MessageResponse(
            message.Id,
            message.Content.Role.ToString(),
            message.Content.Text,
            message.Content.Attachments.Select(a => new FileAttachmentDto(
                a.FileName, a.FileUrl, a.ContentType, a.FileSizeBytes)).ToList(),
            message.Metadata != null ? new MessageMetadataDto(
                message.Metadata.TokenCount,
                message.Metadata.ResponseTime.TotalMilliseconds,
                message.Metadata.ModelUsed,
                message.Metadata.ToolsInvoked) : null,
            message.CreatedAt);
    }
}
EOF

# Query Handlers
cat > src/NetGPT.Application/Handlers/GetConversationHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<ConversationResponse>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        
        if (conversation is null)
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.NotFound", "Conversation not found"));

        if (conversation.UserId != request.UserId)
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.Unauthorized", "Unauthorized"));

        return _mapper.ToResponse(conversation);
    }
}
EOF

cat > src/NetGPT.Application/Handlers/GetConversationsHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetConversationsHandler 
    : IRequestHandler<GetConversationsQuery, Result<PaginatedResponse<ConversationResponse>>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationsHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByUserIdAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);
        
        var total = await _repository.CountByUserIdAsync(request.UserId, cancellationToken);
        
        var data = conversations.Select(_mapper.ToResponse).ToList();
        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        return new PaginatedResponse<ConversationResponse>(
            data, request.Page, request.PageSize, total, totalPages);
    }
}
EOF

cat > src/NetGPT.Application/Handlers/DeleteConversationHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class DeleteConversationHandler : IRequestHandler<DeleteConversationCommand, Result>
{
    private readonly IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConversationHandler(IConversationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        
        if (conversation is null)
            return Result.Failure(new Error("Conversation.NotFound", "Conversation not found"));

        if (conversation.UserId != request.UserId)
            return Result.Failure(new Error("Conversation.Unauthorized", "Unauthorized"));

        await _repository.DeleteAsync(request.ConversationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
EOF

# Validators
cat > src/NetGPT.Application/Validators/CreateConversationValidator.cs << 'EOF'
using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        
        When(x => x.Configuration != null, () =>
        {
            RuleFor(x => x.Configuration!.Temperature)
                .InclusiveBetween(0f, 2f);
            RuleFor(x => x.Configuration!.MaxTokens)
                .GreaterThan(0).LessThanOrEqualTo(128000);
        });
    }
}
EOF

cat > src/NetGPT.Application/Validators/SendMessageValidator.cs << 'EOF'
using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(32000);
    }
}
EOF

echo "Application layer completed"
