// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
