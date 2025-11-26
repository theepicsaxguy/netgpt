using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class AIAgentExecutable : IAgentExecutable
    {
        private readonly AIAgent agent;

        public AIAgentExecutable(AIAgent agent)
        {
            this.agent = agent;
        }

        public async Task<AgentRunResponse> ExecuteAsync(string input, CancellationToken ct = default)
        {
            return await agent.RunAsync(input, cancellationToken: ct);
        }
    }
}
