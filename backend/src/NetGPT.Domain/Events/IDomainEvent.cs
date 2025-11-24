
namespace NetGPT.Domain.Events
{
    using System;

    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
