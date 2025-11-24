
namespace NetGPT.Infrastructure.Tools
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Microsoft.Extensions.AI;

    public sealed class ToolRegistry : IToolRegistry
    {
        private readonly ConcurrentDictionary<string, AIFunction> tools = new();

        public void RegisterTool(AIFunction tool)
        {
            _ = this.tools.TryAdd(tool.Name, tool);
        }

        public IEnumerable<AIFunction> GetAllTools()
        {
            return this.tools.Values;
        }

        public AIFunction? GetTool(string name)
        {
            return this.tools.TryGetValue(name, out AIFunction? tool) ? tool : null;
        }
    }
}
