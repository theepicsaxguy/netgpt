using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Application.Interfaces;

public interface IConversationMapper
{
    ConversationResponse ToResponse(Conversation conversation);
    MessageResponse ToMessageResponse(Message message);
}
