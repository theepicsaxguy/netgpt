#!/bin/bash

# Create Infrastructure project file with Agent Framework packages
cat > src/NetGPT.Infrastructure/NetGPT.Infrastructure.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\NetGPT.Domain\NetGPT.Domain.csproj" />
    <ProjectReference Include="..\NetGPT.Application\NetGPT.Application.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.0.0-preview" />
    <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0-preview" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="OpenAI" Version="2.1.0-beta.1" />
  </ItemGroup>

</Project>
EOF

# Create Agent Orchestrator with Microsoft.Agents.AI
cat > src/NetGPT.Infrastructure/Agents/AgentOrchestrator.cs << 'EOF'
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;
using OpenAI;

namespace NetGPT.Infrastructure.Agents;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IAgentFactory _agentFactory;
    private readonly IToolRegistry _toolRegistry;

    public AgentOrchestrator(IAgentFactory agentFactory, IToolRegistry toolRegistry)
    {
        _agentFactory = agentFactory;
        _toolRegistry = toolRegistry;
    }

    public async Task<Result<AgentResponse>> ExecuteAsync(
        Conversation conversation,
        string userMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var agent = await _agentFactory.CreatePrimaryAgentAsync(
                conversation.AgentConfiguration,
                _toolRegistry.GetAllTools());

            var messages = BuildChatHistory(conversation);
            messages.Add(new ChatMessage(ChatRole.User, userMessage));

            var startTime = DateTime.UtcNow;
            var responseText = string.Empty;
            var toolsInvoked = new List<string>();
            var tokenCount = 0;

            await foreach (var update in agent.RunStreamAsync(messages, cancellationToken))
            {
                if (update.Text != null)
                {
                    responseText += update.Text;
                }

                if (update.ToolCalls != null)
                {
                    foreach (var toolCall in update.ToolCalls)
                    {
                        toolsInvoked.Add(toolCall.FunctionName);
                    }
                }
            }

            var responseTime = DateTime.UtcNow - startTime;
            tokenCount = EstimateTokens(userMessage + responseText);

            return new AgentResponse(
                responseText,
                tokenCount,
                responseTime,
                toolsInvoked);
        }
        catch (Exception ex)
        {
            return Result.Failure<AgentResponse>(
                new Error("AgentOrchestrator.ExecutionFailed", ex.Message));
        }
    }

    private static List<ChatMessage> BuildChatHistory(Conversation conversation)
    {
        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(
                m.Content.Role == MessageRole.User ? ChatRole.User : ChatRole.Assistant,
                m.Content.Text))
            .ToList();
    }

    private static int EstimateTokens(string text) => (int)(text.Length / 4.0);
}

public sealed record AgentResponse(
    string Text,
    int TokensUsed,
    TimeSpan ResponseTime,
    List<string> ToolsInvoked);
EOF

# Create Agent Factory
cat > src/NetGPT.Infrastructure/Agents/AgentFactory.cs << 'EOF'
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
EOF

echo "Agent Framework integration created"
