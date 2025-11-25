// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    internal sealed class AgentRunResponseUpdate
    {
        public AgentRunResponseUpdate(string content, bool isComplete = false)
        {
            Content = content;
            IsComplete = isComplete;
        }

        public string Content { get; }

        public bool IsComplete { get; }
    }
}
