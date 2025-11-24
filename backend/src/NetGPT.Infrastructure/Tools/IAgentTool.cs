// Copyright (c) 2025 NetGPT. All rights reserved.

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
