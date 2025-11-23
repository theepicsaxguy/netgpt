using System.Collections.Generic;

namespace NetGPT.Domain.ValueObjects;

public record ConversationMetadata(
    string? ModelName = null,
    float? Temperature = null,
    int? MaxTokens = null,
    Dictionary<string, object>? CustomProperties = null)
{
    public static ConversationMetadata Default() => new(
        ModelName: "gpt-4o",
        Temperature: 0.7f,
        MaxTokens: 4000
    );
}