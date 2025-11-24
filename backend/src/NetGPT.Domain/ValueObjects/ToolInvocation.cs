// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    using System;

    public record ToolInvocation(
        string ToolName,
        string Arguments,
        string? Result,
        DateTime InvokedAt,
        TimeSpan Duration);
}
