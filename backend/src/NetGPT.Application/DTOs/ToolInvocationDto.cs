// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System;

    public record ToolInvocationDto(
        string ToolName,
        string Arguments,
        string? Result,
        DateTime InvokedAt,
        double DurationMs);
}
