// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents.Workflows
{
    public class WorkflowEngine(
        IOpenAIClientFactory clientFactory) : IWorkflowEngine
    {
        private readonly IOpenAIClientFactory clientFactory = clientFactory;

        public IWorkflow CreateConversationWorkflow()
        {
            return new ConversationWorkflow(clientFactory);
        }
    }
}
