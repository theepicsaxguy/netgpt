using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NetGPT.Infrastructure.Tools;

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string arguments, CancellationToken ct = default);
}