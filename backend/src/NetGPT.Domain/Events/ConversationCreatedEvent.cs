
// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Domain.Events
{
    public record ConversationCreatedEvent(
        ConversationId ConversationId,
        UserId UserId) : IDomainEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
