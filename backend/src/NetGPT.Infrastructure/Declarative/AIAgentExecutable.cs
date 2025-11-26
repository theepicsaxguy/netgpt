using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using NetGPT.Application.Interfaces;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class AIAgentExecutable(AIAgent agent) : IAgentExecutable
    {
        private readonly AIAgent agent = agent;

        public async Task<AgentRunResponse> ExecuteAsync(string input, CancellationToken ct = default)
        {
            return await agent.RunAsync(input, cancellationToken: ct);
        }
    }
}
