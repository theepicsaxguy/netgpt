// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.ValueObjects
{
    public record ToolInvocation(
        string ToolName,
        string Arguments,
        string? Result,
        DateTime InvokedAt,
        TimeSpan Duration);
}
