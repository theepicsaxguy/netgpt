// <copyright file="SendMessageHandler.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.Commands;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Enums;
    using NetGPT.Domain.Interfaces;
    using NetGPT.Domain.Primitives;
    using NetGPT.Domain.ValueObjects;

    public class SendMessageHandler(
        IConversationRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<SendMessageCommand, Result<MessageResponse>>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Result<MessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            ConversationId conversationId = ConversationId.From(request.ConversationId);
            UserId userId = UserId.From(request.UserId);

            Conversation? conversation = await this.repository.GetByIdAsync(conversationId, cancellationToken);
            if (conversation is null)
            {
                return Result.Failure<MessageResponse>(new Error("Conversation.NotFound", "Conversation not found"));
            }

            conversation.EnsureOwnership(userId);

            MessageContent content = MessageContent.FromText(request.Content);
            MessageId messageId = conversation.AddMessage(MessageRole.User, content);

            _ = await this.unitOfWork.SaveChangesAsync(cancellationToken);

            Message message = conversation.GetMessage(messageId);

            MessageResponse response = new(
                message.Id.Value,
                message.Role.ToString(),
                message.Content.Text,
                message.CreatedAt);

            return Result.Success(response);
        }
    }
}
