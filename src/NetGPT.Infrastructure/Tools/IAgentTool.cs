using System.ComponentModel;

namespace NetGPT.Infrastructure.Tools;

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string arguments, CancellationToken ct = default);
}