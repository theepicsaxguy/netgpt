// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.Commands;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Interfaces;
    using NetGPT.Domain.Primitives;
    using NetGPT.Domain.ValueObjects;

    public sealed class DeleteConversationHandler(IConversationRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteConversationCommand, Result>
    {
        private readonly IConversationRepository repository = repository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Result> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
        {
            ConversationId conversationId = ConversationId.From(request.ConversationId);
            UserId userId = UserId.From(request.UserId);

            Conversation? conversation = await this.repository.GetByIdAsync(conversationId, cancellationToken);
            if (conversation is null)
            {
                return Result.Failure(new Error("Conversation.NotFound", "Conversation not found"));
            }

            if (conversation.UserId != userId)
            {
                return Result.Failure(new Error("Conversation.Unauthorized", "Unauthorized access"));
            }

            await this.repository.DeleteAsync(conversationId, cancellationToken);
            _ = await this.unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
