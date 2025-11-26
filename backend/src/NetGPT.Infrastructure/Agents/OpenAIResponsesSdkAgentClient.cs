// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Agents
{
    /// <summary>
    /// SDK-backed adapter that uses the registered <see cref="IOpenAIClientFactory"/> to create an
    /// <see cref="IChatClient"/> and stream responses via the SDK's streaming extensions.
    /// This adapter converts SDK streaming updates into the repository's <see cref="AgentRunResponseUpdate"/> DTOs.
    /// </summary>
    internal sealed class OpenAIResponsesSdkAgentClient(IOpenAIClientFactory chatClientFactory) : AgentClientBase
    {
        private readonly IOpenAIClientFactory chatClientFactory = chatClientFactory;

        public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
            string agentName,
            IList<ChatMessage> messages,
            string? threadId = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Obtain a chat client for the desired model/agent name
            IChatClient chatClient = chatClientFactory.CreateChatClient(agentName);

            // Map our lightweight ChatMessage -> Microsoft.Extensions.AI.ChatMessage
            List<Microsoft.Extensions.AI.ChatMessage> sdkMessages = new(messages.Count);
            foreach (ChatMessage m in messages)
            {
                var role = string.IsNullOrWhiteSpace(m.Role) ? "user" : m.Role!;
                ChatRole chatRole = role.ToLowerInvariant() switch
                {
                    "system" => Microsoft.Extensions.AI.ChatRole.System,
                    "assistant" => Microsoft.Extensions.AI.ChatRole.Assistant,
                    _ => Microsoft.Extensions.AI.ChatRole.User,
                };

                sdkMessages.Add(new Microsoft.Extensions.AI.ChatMessage(chatRole, m.Content ?? string.Empty));
            }

            ChatOptions chatOptions = new()
            {
                ConversationId = threadId,
            };

            // Stream updates from SDK and convert to AgentRunResponseUpdate
            await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(sdkMessages, chatOptions, cancellationToken: cancellationToken))
            {
                // The SDK's ChatResponseUpdate exposes text/content depending on update kind.
                // Convert it to string form for the existing DTO. Consumers can later be
                // updated to consume richer types if necessary.
                yield return new AgentRunResponseUpdate(update?.ToString() ?? string.Empty, isComplete: false);
            }
        }
    }
}
