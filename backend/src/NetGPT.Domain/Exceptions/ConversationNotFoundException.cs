// <copyright file="ConversationNotFoundException.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Domain.Exceptions
{
    public sealed class ConversationNotFoundException(object conversationId) : NotFoundException("Conversation", conversationId)
    {
    }
}
