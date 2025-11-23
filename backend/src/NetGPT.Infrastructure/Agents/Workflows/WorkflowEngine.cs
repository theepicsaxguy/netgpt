using NetGPT.Infrastructure.Tools;

namespace NetGPT.Infrastructure.Agents.Workflows;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly IOpenAIClientFactory _clientFactory;
    private readonly ToolRegistry _toolRegistry;

    public WorkflowEngine(
        IOpenAIClientFactory clientFactory,
        ToolRegistry toolRegistry)
    {
        _clientFactory = clientFactory;
        _toolRegistry = toolRegistry;
    }

    public IWorkflow CreateConversationWorkflow()
    {
        return new ConversationWorkflow(_clientFactory, _toolRegistry);
    }
}