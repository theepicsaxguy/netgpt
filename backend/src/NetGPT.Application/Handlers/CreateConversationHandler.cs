// <copyright file="CreateConversationHandler.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.Commands;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Interfaces;
    using NetGPT.Domain.Primitives;
    using NetGPT.Domain.ValueObjects;

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

            _ = await this.repository.AddAsync(conversation, cancellationToken);
            _ = await this.unitOfWork.SaveChangesAsync(cancellationToken);

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
