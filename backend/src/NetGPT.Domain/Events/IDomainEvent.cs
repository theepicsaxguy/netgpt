using System;

namespace NetGPT.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}