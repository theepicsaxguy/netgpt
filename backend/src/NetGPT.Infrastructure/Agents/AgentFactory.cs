using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Configuration;
using OpenAI;

namespace NetGPT.Infrastructure.Agents;

public interface IAgentFactory
{
    Task<AIAgent> CreatePrimaryAgentAsync(
        AgentConfiguration config,
        IEnumerable<AIFunction> tools);
}

public sealed class AgentFactory : IAgentFactory
{
    private readonly OpenAISettings _settings;

    public AgentFactory(IOptions<OpenAISettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<AIAgent> CreatePrimaryAgentAsync(
        AgentConfiguration config,
        IEnumerable<AIFunction> tools)
    {
        var client = new OpenAIClient(_settings.ApiKey);
        var chatClient = client.GetChatClient(config.ModelName);

        // Cast to IChatClient to use extension method
        IChatClient aiChatClient = chatClient.AsIChatClient();

        var agent = aiChatClient.CreateAIAgent(
            instructions: "You are a helpful AI assistant. Use available tools when needed to help the user.");

        return await Task.FromResult(agent);
    }
}
