using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using NetGPT.Infrastructure.Declarative.Models;
using NetGPT.Infrastructure.Tools;
using Microsoft.Extensions.AI;
using NetGPT.Infrastructure.Agents;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class DeclarativeLoader : IDeclarativeLoader
    {
        private readonly ILogger<DeclarativeLoader> logger;
        private readonly IToolRegistry toolRegistry;
        private readonly IAgentFactory agentFactory;
        private readonly IOpenAIClientFactory? chatClientFactory;
        private readonly DeclarativeCache cache;

        public DeclarativeLoader(
            ILogger<DeclarativeLoader> logger,
            IToolRegistry toolRegistry,
            IAgentFactory agentFactory,
            DeclarativeCache cache,
            IOpenAIClientFactory? chatClientFactory = null)
        {
            this.logger = logger;
            this.toolRegistry = toolRegistry;
            this.agentFactory = agentFactory;
            this.chatClientFactory = chatClientFactory;
            this.cache = cache;
        }

        public async Task<IAgentExecutable> LoadAsync(DefinitionEntity definition, CancellationToken cancellationToken = default)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            string cacheKey = $"{definition.Name}:{definition.Version}";
            if (cache.TryGet<IAgentExecutable>(cacheKey, out var cached))
            {
                return cached!;
            }

            // Parse YAML
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            DefinitionModel model;
            try
            {
                model = deserializer.Deserialize<DefinitionModel>(definition.ContentYaml) ?? new DefinitionModel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse declarative definition YAML for {Name}", definition.Name);
                throw new InvalidOperationException("YAML parse failed", ex);
            }

            // Validate required keys
            if (string.IsNullOrWhiteSpace(model.Type) || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new InvalidOperationException("Definition missing required fields 'type' or 'name'.");
            }

            // Resolve tools
            var resolvedTools = Enumerable.Empty<Microsoft.Extensions.AI.AIFunction>();
            if (model.Tools is { } tools && tools.Any())
            {
                var list = new System.Collections.Generic.List<Microsoft.Extensions.AI.AIFunction>();
                var missing = new System.Collections.Generic.List<string>();
                foreach (string tname in tools)
                {
                    var tf = toolRegistry.GetTool(tname);
                    if (tf == null)
                    {
                        missing.Add(tname);
                    }
                    else
                    {
                        list.Add(tf);
                    }
                }

                if (missing.Any())
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
            var agentDef = new NetGPT.Domain.ValueObjects.AgentDefinition(
                model.Name,
                model.Instructions ?? string.Empty,
                model.Model ?? "gpt-4o");

            AIAgent agent = await agentFactory.CreateAgentAsync(agentDef, resolvedTools);

            var wrapper = new AIAgentExecutable(agent);

            // On new version, evict any older entries for the same name
            cache.EvictByPrefix(definition.Name + ":");

            // Cache the new wrapper
            cache.Set(cacheKey, wrapper);

            return wrapper;
        }
    }
}
