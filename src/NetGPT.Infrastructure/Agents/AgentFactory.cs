using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Configuration;
using OpenAI;

namespace NetGPT.Infrastructure.Agents;

public interface IAgentFactory
{
    Task<ChatAgent> CreatePrimaryAgentAsync(
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

    public async Task<ChatAgent> CreatePrimaryAgentAsync(
        AgentConfiguration config,
        IEnumerable<AIFunction> tools)
    {
        var client = new OpenAIClient(_settings.ApiKey);
        var chatClient = client.GetChatClient(config.ModelName);

        var agent = new ChatAgent(
            chatClient: chatClient,
            instructions: "You are a helpful AI assistant. Use available tools when needed to help the user.",
            tools: tools.ToList());

        return await Task.FromResult(agent);
    }
}
