// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Infrastructure.Agents
{
    public interface IAgentFactory
    {
        Task<AIAgent> CreatePrimaryAgentAsync(
            AgentConfiguration config,
            IEnumerable<AIFunction> tools);

        Task<AIAgent> CreateAgentAsync(
            AgentDefinition definition,
            IEnumerable<AIFunction> tools);
    }
}
