using System;

namespace NetGPT.Domain.ValueObjects;

public record ToolInvocation(
    string ToolName,
    string Arguments,
    string? Result,
    DateTime InvokedAt,
    TimeSpan Duration);