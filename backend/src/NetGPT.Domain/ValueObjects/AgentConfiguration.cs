// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    public record AgentConfiguration(
        string ModelName = "gpt-4o",
        float Temperature = 0.7f,
        int MaxTokens = 4000,
        float? TopP = null,
        float? FrequencyPenalty = null,
        float? PresencePenalty = null)
    {
        public static AgentConfiguration Default()
        {
            return new();
        }
    }
}
