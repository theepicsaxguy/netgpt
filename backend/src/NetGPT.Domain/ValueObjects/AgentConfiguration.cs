// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    public record AgentDefinition(
        string Name,
        string Instructions,
        string ModelName = "gpt-4o",
        float Temperature = 0.7f,
        int MaxTokens = 4000);

    public record AgentConfiguration(
        string ModelName = "gpt-4o",
        float Temperature = 0.7f,
        int MaxTokens = 4000,
        float? TopP = null,
        float? FrequencyPenalty = null,
        float? PresencePenalty = null,
        IReadOnlyList<AgentDefinition>? Agents = null)
    {
        public static AgentConfiguration Default()
        {
            return new();
        }

        public bool IsMultiAgent => Agents is not null && Agents.Count > 1;
    }
}
