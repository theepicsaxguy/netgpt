using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Tools;

public interface IToolRegistry
{
    void RegisterTool(AIFunction tool);
    IEnumerable<AIFunction> GetAllTools();
    AIFunction? GetTool(string name);
}
