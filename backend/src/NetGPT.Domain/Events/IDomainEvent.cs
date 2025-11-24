// <copyright file="IDomainEvent.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Domain.Events
{
    using System;

    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
