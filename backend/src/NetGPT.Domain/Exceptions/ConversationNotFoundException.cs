
namespace NetGPT.Domain.Exceptions
{
    public sealed class ConversationNotFoundException(object conversationId) : NotFoundException("Conversation", conversationId)
    {
    }
}
