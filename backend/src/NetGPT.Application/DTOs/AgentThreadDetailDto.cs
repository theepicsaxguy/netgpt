using System;
using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record ToolInvocationDetailDto(
    Guid Id,
    string ToolName,
    string Arguments,
    string? Result,
    DateTime InvokedAt);

public record StreamingChunkDetailDto(
    Guid Id,
    Guid? MessageId,
    string Content,
    bool IsComplete,
    DateTime EmittedAt);

public record AgentThreadDetailDto(
    Guid Id,
    Guid ConversationId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    IEnumerable<ToolInvocationDetailDto> ToolInvocations,
    IEnumerable<StreamingChunkDetailDto> StreamingChunks);
