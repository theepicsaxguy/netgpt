using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace NetGPT.Infrastructure.Declarative
{
    public interface IAgentExecutable
    {
        Task<AgentRunResponse> ExecuteAsync(string input, CancellationToken ct = default);
    }
}
