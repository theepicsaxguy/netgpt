
namespace NetGPT.Application.Interfaces
{
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Aggregates;

    public interface IConversationMapper
    {
        ConversationResponse ToResponse(Conversation conversation);

        MessageResponse ToMessageResponse(Message message);
    }
}
