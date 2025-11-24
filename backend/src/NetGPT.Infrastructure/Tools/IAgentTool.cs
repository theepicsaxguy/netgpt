// <copyright file="IAgentTool.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Infrastructure.Tools
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAgentTool
    {
        string Name { get; }

        string Description { get; }

        Task<string> ExecuteAsync(string arguments, CancellationToken ct = default);
    }
}
