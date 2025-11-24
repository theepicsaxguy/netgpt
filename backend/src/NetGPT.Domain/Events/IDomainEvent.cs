// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Events
{
    using System;

    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
