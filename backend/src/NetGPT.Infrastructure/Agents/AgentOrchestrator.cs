// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Services;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Primitives;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Tools;

namespace NetGPT.Infrastructure.Agents
{
    /// <summary>
    /// Orchestrates AI agent execution for conversations and declarative definitions.
    /// </summary>
    public sealed partial class AgentOrchestrator(IAgentFactory agentFactory, IToolRegistry toolRegistry, ILogger<AgentOrchestrator> logger) : IAgentOrchestrator
    {
        private static readonly Action<ILogger, string, double, string, Exception?> EvaluationLogged = LoggerMessage.Define<string, double, string>(
            LogLevel.Information,
            new EventId(1, "Evaluation"),
            "Evaluation {Evaluator}: Score {Score}, Feedback {Feedback}");

        private static readonly Action<ILogger, Guid, int, Guid, DateTime, DateTime, Exception?> DeclarativeExecutionFinished = LoggerMessage.Define<Guid, int, Guid, DateTime, DateTime>(
            LogLevel.Information,
            new EventId(2, "DeclarativeExecutionFinished"),
            "Declarative execution finished definitionId={DefinitionId} version={Version} executionId={ExecutionId} start={Start} end={End} outcome=success");

        private static readonly Action<ILogger, Guid, int, Guid, Exception?> DeclarativeExecutionFailed = LoggerMessage.Define<Guid, int, Guid>(
            LogLevel.Error,
            new EventId(3, "DeclarativeExecutionFailed"),
            "Declarative execution failed definitionId={DefinitionId} version={Version} executionId={ExecutionId}");

        private readonly IAgentFactory agentFactory = agentFactory;
        private readonly IToolRegistry toolRegistry = toolRegistry;
        private readonly ILogger<AgentOrchestrator> logger = logger;
        private readonly List<IEvaluator> evaluators = [new RelevanceEvaluator(), new ToolCallAccuracyEvaluator()];

        public async Task<Result<AgentResponse>> ExecuteAsync(
            Conversation conversation,
            string userMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                DateTime startTime = DateTime.UtcNow;
                string responseText = string.Empty;
                int tokenCount = 0;

                if (conversation.AgentConfiguration.IsMultiAgent)
                {
                    // Simple multi-agent: each agent responds sequentially, concatenate responses
                    List<AIAgent> agents = [];
                    if (conversation.AgentConfiguration.Agents is { } agentDefs)
                    {
                        foreach (AgentDefinition agentDef in agentDefs)
                        {
                            AIAgent agent = await agentFactory.CreateAgentAsync(agentDef, toolRegistry.GetAllTools());
                            agents.Add(agent);
                        }
                    }

                    List<string> responses = [];
                    foreach (AIAgent agent in agents)
                    {
                        AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);
                        string agentResponse = result.Messages.LastOrDefault()?.Text ?? string.Empty;
                        responses.Add($"{agent.Name}: {agentResponse}");
                    }

                    responseText = string.Join("\n\n", responses);
                }
                else
                {
                    // Single-agent execution
                    AIAgent agent = await agentFactory.CreatePrimaryAgentAsync(
                        conversation.AgentConfiguration,
                        toolRegistry.GetAllTools());

                    // Run agent with user message
                    AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);

                    // Extract the text response from AgentRunResponse
                    responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;
                }

                TimeSpan responseTime = DateTime.UtcNow - startTime;
                tokenCount = EstimateTokens(userMessage + responseText);

                AgentResponse response = new(
                    Content: responseText,
                    TokensUsed: tokenCount,
                    ResponseTime: responseTime,
                    ModelUsed: conversation.AgentConfiguration.ModelName);

                // Run evaluators
                foreach (IEvaluator evaluator in evaluators)
                {
                    EvaluationResult evalResult = await evaluator.EvaluateAsync(conversation, userMessage, response);
                    EvaluationLogged(logger, evalResult.EvaluatorName, evalResult.Score, evalResult.Feedback, null);
                }

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<AgentResponse>(
                    new DomainError("AgentOrchestrator.ExecutionFailed", ex.Message));
            }
        }

        public async IAsyncEnumerable<StreamingChunkDto> ExecuteStreamingAsync(
            Conversation conversation,
            string userMessage,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Guid messageId = Guid.NewGuid();

            if (conversation.AgentConfiguration.IsMultiAgent)
            {
                // Multi-agent: stream each agent's response
                List<AIAgent> agents = [];
                if (conversation.AgentConfiguration.Agents is { } agentDefs)
                {
                    foreach (AgentDefinition agentDef in agentDefs)
                    {
                        AIAgent agent = await agentFactory.CreateAgentAsync(agentDef, toolRegistry.GetAllTools());
                        agents.Add(agent);
                    }
                }

                foreach (AIAgent agent in agents)
                {
                    AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);
                    string agentResponse = result.Messages.LastOrDefault()?.Text ?? string.Empty;
                    string chunk = $"{agent.Name}: {agentResponse}\n\n";
                    yield return new StreamingChunkDto(messageId, chunk, false, DateTime.UtcNow);
                }
            }
            else
            {
                // Single-agent: stream the response
                AIAgent agent = await agentFactory.CreatePrimaryAgentAsync(
                    conversation.AgentConfiguration,
                    toolRegistry.GetAllTools());

                AgentRunResponse result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);
                string responseText = result.Messages.LastOrDefault()?.Text ?? string.Empty;

                // Yield in chunks (simple split by sentences)
                string[] chunks = responseText.Split('.', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < chunks.Length; i++)
                {
                    string chunk = chunks[i] + (i < chunks.Length - 1 ? "." : string.Empty);
                    yield return new StreamingChunkDto(messageId, chunk, i == chunks.Length - 1, DateTime.UtcNow);
                    await Task.Delay(50, cancellationToken); // Simulate streaming delay
                }
            }

            // Run evaluators after streaming
            Result<AgentResponse> fullResult = await ExecuteAsync(conversation, userMessage, cancellationToken);
            if (fullResult.IsSuccess)
            {
                foreach (IEvaluator evaluator in evaluators)
                {
                    EvaluationResult evalResult = await evaluator.EvaluateAsync(conversation, userMessage, fullResult.Value);
                    EvaluationLogged(logger, evalResult.EvaluatorName, evalResult.Score, evalResult.Feedback, null);
                }
            }
        }

        private static int EstimateTokens(string text)
        {
            return (int)(text.Length / 4.0);
        }
    }
}
