
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
