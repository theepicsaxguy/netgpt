using System;

namespace NetGPT.Application.DTOs;

public record ToolInvocationDto(
    string ToolName,
    string Arguments,
    string? Result,
    DateTime InvokedAt,
    double DurationMs);