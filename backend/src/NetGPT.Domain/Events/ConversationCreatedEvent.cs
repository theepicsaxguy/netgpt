// <copyright file="ConversationCreatedEvent.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

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
