// <copyright file="IWorkflowEngine.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Infrastructure.Agents.Workflows
{
    using System.Collections.Generic;
    using System.Threading;
    using NetGPT.Application.DTOs;

    public interface IWorkflowEngine
    {
        IWorkflow CreateConversationWorkflow();
    }

    public interface IWorkflow
    {
        IAsyncEnumerable<StreamingChunkDto> ExecuteAsync(
            WorkflowContext context,
            CancellationToken ct = default);
    }
}
