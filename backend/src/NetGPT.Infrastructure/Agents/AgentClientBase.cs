// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Threading;

namespace NetGPT.Infrastructure.Agents
{
    internal abstract class AgentClientBase
    {
        public abstract IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
            string agentName,
            IList<ChatMessage> messages,
            string? threadId = null,
            CancellationToken cancellationToken = default);
    }
}
