// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NetGPT.Application.DTOs;
using OpenAI;

namespace NetGPT.Infrastructure.Agents.Workflows
{
    public class ConversationWorkflow(
        IOpenAIClientFactory clientFactory) : IWorkflow
    {
        private readonly IOpenAIClientFactory clientFactory = clientFactory;

        public async IAsyncEnumerable<StreamingChunkDto> ExecuteAsync(
            WorkflowContext context,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            IChatClient chatClient = clientFactory.CreateChatClient();

            ChatClientAgent agent = chatClient.CreateAIAgent(
                instructions: "You are a helpful AI assistant. Use tools when needed.");

            // Simple non-streaming response for now
            AgentRunResponse result = await agent.RunAsync(context.UserMessage, cancellationToken: ct);
            string responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

            yield return new StreamingChunkDto(
                ChunkId: Guid.NewGuid(),
                Text: responseText,
                IsFinal: false,
                CreatedAt: DateTime.UtcNow);

            yield return new StreamingChunkDto(
                ChunkId: Guid.NewGuid(),
                Text: string.Empty,
                IsFinal: true,
                CreatedAt: DateTime.UtcNow);
        }
    }
}
