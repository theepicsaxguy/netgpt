// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Entities;
using NetGPT.Domain.ValueObjects;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Declarative.Models;
using NetGPT.Infrastructure.Tools;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed partial class DeclarativeLoader(
        ILogger<DeclarativeLoader> logger,
        IToolRegistry toolRegistry,
        IAgentFactory agentFactory,
        DeclarativeCache cache,
        IOpenAIClientFactory? chatClientFactory = null) : IDeclarativeLoader
    {
        private readonly ILogger<DeclarativeLoader> logger = logger;
        private readonly IToolRegistry toolRegistry = toolRegistry;
        private readonly IAgentFactory agentFactory = agentFactory;
        private readonly IOpenAIClientFactory? chatClientFactory = chatClientFactory;
        private readonly DeclarativeCache cache = cache;

        public async Task<IAgentExecutable> LoadAsync(DefinitionEntity definition, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(definition);

            string cacheKey = $"{definition.Name}:{definition.Version}";
            if (cache.TryGet(cacheKey, out IAgentExecutable? cached))
            {
                return cached!;
            }

            // Parse YAML
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            DefinitionModel model;
            try
            {
                model = deserializer.Deserialize<DefinitionModel>(definition.ContentYaml) ?? new DefinitionModel();
            }
            catch (Exception ex)
            {
                LogParseError(logger, ex, definition.Name);
                throw new InvalidOperationException("YAML parse failed", ex);
            }

            // Validate required keys
            if (string.IsNullOrWhiteSpace(model.Type) || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new InvalidOperationException("Definition missing required fields 'type' or 'name'.");
            }

            // Resolve tools
            IEnumerable<AIFunction> resolvedTools = [];
            if (model.Tools is { } tools && tools.Count > 0)
            {
                List<AIFunction> list = [];
                List<string> missing = [];
                foreach (string tname in tools)
                {
                    AIFunction? tf = toolRegistry.GetTool(tname);
                    if (tf == null)
                    {
                        missing.Add(tname);
                    }
                    else
                    {
                        list.Add(tf);
                    }
                }

                if (missing.Count > 0)
                {
                    throw new InvalidOperationException($"Unknown tool(s) referenced: {string.Join(',', missing)}");
                }

                resolvedTools = list;
            }

            // If the type implies a chat client provider, ensure chat client factory is available
            bool requiresChatClient = model.Type.Equals("openai", StringComparison.OrdinalIgnoreCase)
                || model.Type.Equals("azureai", StringComparison.OrdinalIgnoreCase)
                || model.Type.Equals("foundry_agent", StringComparison.OrdinalIgnoreCase);

            if (requiresChatClient && chatClientFactory == null)
            {
                throw new InvalidOperationException($"Missing IChatClient provider for definition '{definition.Name}'");
            }

            // Map to AgentDefinition and create agent via factory
            AgentDefinition agentDef = new(
                model.Name,
                model.Instructions ?? string.Empty,
                model.Model ?? "gpt-4o");

            AIAgent agent = await agentFactory.CreateAgentAsync(agentDef, resolvedTools);

            AIAgentExecutable wrapper = new(agent);

            // On new version, evict any older entries for the same name
            cache.EvictByPrefix(definition.Name + ":");

            // Cache the new wrapper
            cache.Set(cacheKey, wrapper);

            return wrapper;
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to parse declarative definition YAML for {Name}")]
        private static partial void LogParseError(ILogger logger, Exception? ex, string name);
    }
}
