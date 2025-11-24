namespace NetGPT.Domain.Exceptions;

public sealed class ConversationNotFoundException : NotFoundException
{
    public ConversationNotFoundException(object conversationId)
        : base("Conversation", conversationId) { }
}
