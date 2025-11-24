// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record ToolInvocationDetailDto(
        Guid Id,
        string ToolName,
        string Arguments,
        string? Result,
        DateTime InvokedAt);
}