// Copyright (c) 2025 NetGPT. All rights reserved.

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
            Conversation conversation = Conversation.Create(userId, request.Title);

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
    }
}
