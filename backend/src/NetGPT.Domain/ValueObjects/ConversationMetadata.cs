// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    using System.Collections.Generic;

    public record ConversationMetadata(
        string? ModelName = null,
        float? Temperature = null,
        int? MaxTokens = null,
        Dictionary<string, object>? CustomProperties = null)
    {
        public static ConversationMetadata Default()
        {
            return new(
            ModelName: "gpt-4o",
            Temperature: 0.7f,
            MaxTokens: 4000);
        }
    }
}
