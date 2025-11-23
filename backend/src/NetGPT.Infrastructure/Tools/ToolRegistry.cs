using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, AIFunction> _tools = new();

    public void RegisterTool(AIFunction tool)
    {
        _tools.TryAdd(tool.Metadata.Name, tool);
    }

    public IEnumerable<AIFunction> GetAllTools() => _tools.Values;

    public AIFunction? GetTool(string name) =>
        _tools.TryGetValue(name, out var tool) ? tool : null;
}
