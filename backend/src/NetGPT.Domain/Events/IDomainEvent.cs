// <copyright file="IDomainEvent.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.Events
{
    using System;

    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
