using System.Collections.Generic;
using System.Threading;
using NetGPT.Application.DTOs;

namespace NetGPT.Infrastructure.Agents.Workflows;

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