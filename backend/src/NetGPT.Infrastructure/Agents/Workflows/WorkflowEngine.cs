// <copyright file="WorkflowEngine.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

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
