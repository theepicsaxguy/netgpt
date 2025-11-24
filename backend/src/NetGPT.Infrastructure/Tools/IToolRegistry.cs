// <copyright file="IToolRegistry.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Infrastructure.Tools
{
    using System.Collections.Generic;
    using Microsoft.Extensions.AI;

    public interface IToolRegistry
    {
        void RegisterTool(AIFunction tool);

        IEnumerable<AIFunction> GetAllTools();

        AIFunction? GetTool(string name);
    }
}
