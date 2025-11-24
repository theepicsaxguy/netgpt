// <copyright file="ToolInvocationDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

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
