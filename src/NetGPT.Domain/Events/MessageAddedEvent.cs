using NetGPT.Domain.ValueObjects;
using NetGPT.Domain.Enums;

namespace NetGPT.Domain.Events;

public record MessageAddedEvent(
    ConversationId ConversationId,
    MessageId MessageId,
    MessageRole Role) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}