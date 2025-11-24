// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    using Microsoft.Extensions.AI;
    using NetGPT.Infrastructure.Configuration;
    using OpenAI;
    using OpenAI.Chat;

    public interface IOpenAIClientFactory
    {
        IChatClient CreateChatClient(string? model = null);
    }

    public class OpenAIClientFactory(OpenAISettings settings) : IOpenAIClientFactory
    {
        private readonly OpenAISettings settings = settings;

        public IChatClient CreateChatClient(string? model = null)
        {
            OpenAIClient client = new(this.settings.ApiKey);
            ChatClient chatClient = client.GetChatClient(model ?? this.settings.DefaultModel);
            return chatClient.AsIChatClient();
        }
    }
}
