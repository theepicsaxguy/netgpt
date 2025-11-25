// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Concurrent;
using System.Text.Json;
using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.A2A.Converters;
using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Agents;

internal sealed class A2AAgentClient(ILogger<A2AAgentClient> logger, Uri baseUri) : AgentClientBase
{
    private readonly ILogger _logger = logger;
    private readonly Uri _uri = baseUri;

    // a client per agent name
    private readonly ConcurrentDictionary<string, (A2AClient, A2ACardResolver)> _clients = new();

    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        string agentName,
        IList<ChatMessage> messages,
        string? threadId = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Running agent {AgentName} with {MessageCount} messages via A2A", agentName, messages?.Count ?? 0);

        var (a2aClient, _) = ResolveClient(agentName);
        var contextId = threadId ?? Guid.NewGuid().ToString("N");

        try
        {
            var parts = messages.ToParts();
            var a2aMessage = new AgentMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ContextId = contextId,
                Role = MessageRole.User,
                Parts = parts
            };

            var messageSendParams = new MessageSendParams { Message = a2aMessage };
            var a2aResponse = await a2aClient.SendMessageAsync(messageSendParams, cancellationToken);

            if (a2aResponse is AgentMessage am)
            {
                var responseMessage = am.ToChatMessage();
                if (responseMessage is { Contents.Count: > 0 })
                {
                    yield return new AgentRunResponseUpdate(responseMessage.ContentsToString(), isComplete: true);
                }
            }
            else if (a2aResponse is AgentTask agentTask)
            {
                if (agentTask.Artifacts is not null)
                {
                    foreach (var artifact in agentTask.Artifacts)
                    {
                        List<AIContent>? aiContents = null;
                        foreach (var part in artifact.Parts)
                        {
                            (aiContents ??= []).Add(part.ToAIContent());
                        }

                        if (aiContents is not null)
                        {
                            var additionalProperties = ConvertMetadataToAdditionalProperties(artifact.Metadata);
                            var chatMessage = new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, aiContents.ToString() ?? string.Empty);

                            yield return new AgentRunResponseUpdate(aiContentsToString(aiContents), isComplete: true);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Unsupported A2A response type: {ResponseType}", a2aResponse?.GetType().FullName ?? "null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running agent {AgentName} via A2A", agentName);
            yield return new AgentRunResponseUpdate($"Error: {ex.Message}", isComplete: true);
        }
    }

    private (A2AClient, A2ACardResolver) ResolveClient(string agentName) =>
        _clients.GetOrAdd(agentName, name =>
        {
            var uri = new Uri($"{_uri}/{name}/");
            var a2aClient = new A2AClient(uri);
            var a2aCardResolver = new A2ACardResolver(uri, agentCardPath: "/v1/card/");
            _logger.LogInformation("Built clients for agent {Agent} with baseUri {Uri}", name, uri);
            return (a2aClient, a2aCardResolver);
        });

    private static string aiContentsToString(List<AIContent> contents)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in contents)
        {
            if (c is TextContent t)
            {
                sb.Append(t.Text);
            }
        }
        return sb.ToString();
    }

    private static AdditionalPropertiesDictionary? ConvertMetadataToAdditionalProperties(Dictionary<string, JsonElement>? metadata)
    {
        if (metadata is not { Count: > 0 })
        {
            return null;
        }

        var additionalProperties = new AdditionalPropertiesDictionary();
        foreach (var kvp in metadata)
        {
            additionalProperties[kvp.Key] = kvp.Value;
        }

        return additionalProperties;
    }
}
