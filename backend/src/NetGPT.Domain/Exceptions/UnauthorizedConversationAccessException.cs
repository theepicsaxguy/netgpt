
namespace NetGPT.Domain.Exceptions
{
    public sealed class UnauthorizedConversationAccessException(object conversationId, object userId) : DomainException($"User {userId} is not authorized to access conversation {conversationId}")
    {
    }
}
