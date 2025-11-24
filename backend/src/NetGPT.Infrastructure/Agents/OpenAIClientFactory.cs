// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.Extensions.AI;
using NetGPT.Infrastructure.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace NetGPT.Infrastructure.Agents
{
    public sealed class OpenAIClientFactory(OpenAISettings settings) : IOpenAIClientFactory
    {
        private readonly OpenAISettings settings = settings;

        public IChatClient CreateChatClient(string? model = null)
        {
            OpenAIClient client = new(settings.ApiKey);
            ChatClient chatClient = client.GetChatClient(model ?? settings.DefaultModel);
            return chatClient.AsIChatClient();
        }
    }
}
