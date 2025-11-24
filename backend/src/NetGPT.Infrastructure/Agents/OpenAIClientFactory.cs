using Microsoft.Extensions.AI;
using NetGPT.Infrastructure.Configuration;
using OpenAI;

namespace NetGPT.Infrastructure.Agents;

public interface IOpenAIClientFactory
{
    IChatClient CreateChatClient(string? model = null);
}

public class OpenAIClientFactory : IOpenAIClientFactory
{
    private readonly OpenAISettings _settings;

    public OpenAIClientFactory(OpenAISettings settings)
    {
        _settings = settings;
    }

    public IChatClient CreateChatClient(string? model = null)
    {
        var client = new OpenAIClient(_settings.ApiKey);
        var chatClient = client.GetChatClient(model ?? _settings.DefaultModel);
        return chatClient.AsIChatClient();
    }
}