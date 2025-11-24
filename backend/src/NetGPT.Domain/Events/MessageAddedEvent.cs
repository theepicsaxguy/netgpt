// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using NetGPT.Domain.Enums;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Domain.Events
{
    public record MessageAddedEvent(
        ConversationId ConversationId,
        MessageId MessageId,
        MessageRole Role) : IDomainEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
