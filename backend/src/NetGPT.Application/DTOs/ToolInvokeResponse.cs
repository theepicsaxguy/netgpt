// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Response from a tool invocation.
    /// </summary>
    public record ToolInvokeResponse(
        string ToolName,
        object? Result,
        bool Success,
        string? ErrorMessage,
        DateTime InvokedAt,
        double DurationMs);
}
