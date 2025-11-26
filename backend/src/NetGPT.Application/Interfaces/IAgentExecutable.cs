// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace NetGPT.Application.Interfaces
{
    public interface IAgentExecutable
    {
        Task<AgentRunResponse> ExecuteAsync(string input, CancellationToken ct = default);
    }
}
