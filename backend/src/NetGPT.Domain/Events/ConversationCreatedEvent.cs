// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Events
{
    using System;
    using NetGPT.Domain.ValueObjects;

    public record ConversationCreatedEvent(
        ConversationId ConversationId,
        UserId UserId) : IDomainEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
