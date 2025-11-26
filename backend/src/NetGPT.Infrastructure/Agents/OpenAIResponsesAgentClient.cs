// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Configuration;
using OpenAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Chat;
using ChatSdkMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatSdkResponseUpdate = Microsoft.Extensions.AI.ChatResponseUpdate;
using ChatSdkRole = Microsoft.Extensions.AI.ChatRole;

namespace NetGPT.Infrastructure.Agents
{
    /// <summary>
    /// SDK-backed adapter that uses the OpenAI SDK and Microsoft Agent extensions to stream responses.
    /// This implementation constructs an <see cref="OpenAIClient"/> using configuration and then uses
    /// the ChatClient/AsIChatClient extension to obtain streaming responses.
    /// </summary>
        internal sealed class OpenAIResponsesAgentClient(IOptions<OpenAISettings> options) : AgentClientBase
        {
            private readonly OpenAISettings settings = options.Value;

        public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
            string agentName,
            IList<ChatMessage> messages,
            string? threadId = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Construct OpenAI client using the configured API key. The OpenAI SDK will talk
            // to the default endpoint unless overridden by configuration in OpenAISettings.
            OpenAIClient client = string.IsNullOrWhiteSpace(settings.ApiKey)
                ? throw new InvalidOperationException("OpenAI ApiKey not configured")
                : new OpenAIClient(settings.ApiKey);

            // Get a ChatClient for the requested model
            ChatClient chatClient = client.GetChatClient(agentName);

            // Cast to the IChatClient that exposes the streaming helper methods used by Microsoft.Agents.AI
            IChatClient aiChatClient = chatClient.AsIChatClient();

            // Map our internal lightweight ChatMessage to the Microsoft.Extensions.AI chat message type
            List<ChatSdkMessage> sdkMessages = new(messages.Count);
            foreach (ChatMessage m in messages)
            {
                string role = string.IsNullOrWhiteSpace(m.Role) ? "user" : m.Role;

                ChatSdkRole chatRole = role.ToLowerInvariant() switch
                {
                    "system" => ChatSdkRole.System,
                    "assistant" => ChatSdkRole.Assistant,
                    _ => ChatSdkRole.User,
                };

                sdkMessages.Add(new ChatSdkMessage(chatRole, m.Content ?? string.Empty));
            }

            ChatOptions chatOptions = new()
            {
                ConversationId = threadId,
            };

            await foreach (ChatSdkResponseUpdate update in aiChatClient.GetStreamingResponseAsync(sdkMessages, chatOptions, cancellationToken: cancellationToken))
            {
                yield return new AgentRunResponseUpdate(update?.ToString() ?? string.Empty, isComplete: false);
            }
        }
    }
}
