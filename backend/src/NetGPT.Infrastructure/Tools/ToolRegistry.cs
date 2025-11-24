// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Tools
{
    public sealed class ToolRegistry : IToolRegistry
    {
        private readonly ConcurrentDictionary<string, AIFunction> tools = new();

        public void RegisterTool(AIFunction tool)
        {
            _ = tools.TryAdd(tool.Name, tool);
        }

        public IEnumerable<AIFunction> GetAllTools()
        {
            return tools.Values;
        }

        public AIFunction? GetTool(string name)
        {
            return tools.TryGetValue(name, out AIFunction? tool) ? tool : null;
        }
    }
}
