// <copyright file="ToolInvocationDto.cs" theepicsaxguy">
// \
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
