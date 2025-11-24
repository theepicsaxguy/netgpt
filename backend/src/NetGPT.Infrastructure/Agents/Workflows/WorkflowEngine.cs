// Copyright (c) 2025 NetGPT. All rights reserved.

using NetGPT.Infrastructure.Tools;

namespace NetGPT.Infrastructure.Agents.Workflows
{
    public class WorkflowEngine(
        IOpenAIClientFactory clientFactory,
        ToolRegistry toolRegistry) : IWorkflowEngine
    {
        private readonly IOpenAIClientFactory clientFactory = clientFactory;
        private readonly ToolRegistry toolRegistry = toolRegistry;

        public IWorkflow CreateConversationWorkflow()
        {
            return new ConversationWorkflow(clientFactory, toolRegistry);
        }
    }
}
