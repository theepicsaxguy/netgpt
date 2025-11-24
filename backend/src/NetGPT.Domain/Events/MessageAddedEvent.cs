// <copyright file="MessageAddedEvent.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.Events
{
    using System;
    using NetGPT.Domain.Enums;
    using NetGPT.Domain.ValueObjects;

    public record MessageAddedEvent(
        ConversationId ConversationId,
        MessageId MessageId,
        MessageRole Role) : IDomainEvent
    {
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}
