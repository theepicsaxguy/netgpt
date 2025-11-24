// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents.Workflows
{
    using NetGPT.Infrastructure.Tools;

    public class WorkflowEngine(
        IOpenAIClientFactory clientFactory,
        ToolRegistry toolRegistry) : IWorkflowEngine
    {
        private readonly IOpenAIClientFactory clientFactory = clientFactory;
        private readonly ToolRegistry toolRegistry = toolRegistry;

        public IWorkflow CreateConversationWorkflow()
        {
            return new ConversationWorkflow(this.clientFactory, this.toolRegistry);
        }
    }
}
