using System.Collections.Generic;
using NetGPT.Domain.Aggregates;
using NetGPT.Infrastructure.Configuration;

namespace NetGPT.Infrastructure.Agents.Workflows;

public class WorkflowContext
{
    public required Conversation Conversation { get; init; }
    public required string UserMessage { get; init; }
    public required OpenAISettings Settings { get; init; }
    public Dictionary<string, object> State { get; } = new();
}