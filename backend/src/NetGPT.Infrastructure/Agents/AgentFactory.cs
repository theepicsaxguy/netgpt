// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Agents
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Agents.AI;
    using Microsoft.Extensions.AI;
    using Microsoft.Extensions.Options;
    using NetGPT.Domain.ValueObjects;
    using NetGPT.Infrastructure.Configuration;
    using OpenAI;
    using OpenAI.Chat;

    public interface IAgentFactory
    {
        Task<AIAgent> CreatePrimaryAgentAsync(
            AgentConfiguration config,
            IEnumerable<AIFunction> tools);
    }

    public sealed class AgentFactory(IOptions<OpenAISettings> settings) : IAgentFactory
    {
        private readonly OpenAISettings settings = settings.Value;

        public async Task<AIAgent> CreatePrimaryAgentAsync(
            AgentConfiguration config,
            IEnumerable<AIFunction> tools)
        {
            OpenAIClient client = new(this.settings.ApiKey);
            ChatClient chatClient = client.GetChatClient(config.ModelName);

            // Cast to IChatClient to use extension method
            IChatClient aiChatClient = chatClient.AsIChatClient();

            ChatClientAgent agent = aiChatClient.CreateAIAgent(
                instructions: "You are a helpful AI assistant. Use available tools when needed to help the user.");

            return await Task.FromResult(agent);
        }
    }
}
