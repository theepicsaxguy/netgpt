namespace NetGPT.Domain.Exceptions;

public sealed class UnauthorizedConversationAccessException : DomainException
{
    public UnauthorizedConversationAccessException(object conversationId, object userId)
        : base($"User {userId} is not authorized to access conversation {conversationId}") { }
}
