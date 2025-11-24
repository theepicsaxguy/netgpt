// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents.Workflows
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Agents.AI;
    using Microsoft.Extensions.AI;
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.ValueObjects;
    using NetGPT.Infrastructure.Tools;
    using OpenAI;

    public class ConversationWorkflow(
        IOpenAIClientFactory clientFactory,
        ToolRegistry toolRegistry) : IWorkflow
    {
        private readonly IOpenAIClientFactory clientFactory = clientFactory;
        private readonly ToolRegistry toolRegistry = toolRegistry;

        public async IAsyncEnumerable<StreamingChunkDto> ExecuteAsync(
            WorkflowContext context,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            IChatClient chatClient = this.clientFactory.CreateChatClient();

            ChatClientAgent agent = chatClient.CreateAIAgent(
                instructions: "You are a helpful AI assistant. Use tools when needed.");

            MessageId messageId = MessageId.CreateNew();

            // Simple non-streaming response for now
            AgentRunResponse result = await agent.RunAsync(context.UserMessage, cancellationToken: ct);
            string responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

            yield return new StreamingChunkDto(
                MessageId: messageId.Value,
                Content: responseText,
                IsComplete: false);

            yield return new StreamingChunkDto(
                MessageId: messageId.Value,
                Content: null,
                IsComplete: true);
        }
    }
}
