// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    internal sealed class AgentRunResponseUpdate(string content, bool isComplete = false)
    {
        public string Content { get; } = content;

        public bool IsComplete { get; } = isComplete;
    }
}
