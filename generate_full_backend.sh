#!/bin/bash

# Complete backend generator for NetGPT

# Fix the commands file
cat > src/NetGPT.Application/Commands/ConversationCommands.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Commands;

public sealed record CreateConversationCommand(
    Guid UserId,
    string? Title,
    AgentConfigurationDto? Configuration) : IRequest<Result<ConversationResponse>>;

public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid UserId,
    string Content,
    List<FileAttachmentDto>? Attachments) : IRequest<Result<MessageResponse>>;

public sealed record SendMessageStreamingCommand(
    Guid ConversationId,
    Guid UserId,
    string Content,
    List<FileAttachmentDto>? Attachments) : IRequest<Result<Guid>>;

public sealed record UpdateConversationCommand(
    Guid ConversationId,
    Guid UserId,
    string Title) : IRequest<Result<ConversationResponse>>;

public sealed record DeleteConversationCommand(
    Guid ConversationId,
    Guid UserId) : IRequest<Result>;

public sealed record RegenerateResponseCommand(
    Guid ConversationId,
    Guid UserId,
    Guid MessageId) : IRequest<Result<MessageResponse>>;
EOF

# Create Queries
cat > src/NetGPT.Application/Queries/ConversationQueries.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Queries;

public sealed record GetConversationQuery(
    Guid ConversationId,
    Guid UserId) : IRequest<Result<ConversationResponse>>;

public sealed record GetConversationsQuery(
    Guid UserId,
    int Page,
    int PageSize) : IRequest<Result<PaginatedResponse<ConversationResponse>>>;

public sealed record GetMessagesQuery(
    Guid ConversationId,
    Guid UserId) : IRequest<Result<List<MessageResponse>>>;

public sealed record SearchConversationsQuery(
    Guid UserId,
    string SearchTerm,
    int Page,
    int PageSize) : IRequest<Result<PaginatedResponse<ConversationResponse>>>;
EOF

echo "Application commands and queries created"
