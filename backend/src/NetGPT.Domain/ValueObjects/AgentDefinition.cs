// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    public record AgentDefinition(
        string Name,
        string Instructions,
        string ModelName = "gpt-4o",
        float Temperature = 0.7f,
        int MaxTokens = 4000);
}
