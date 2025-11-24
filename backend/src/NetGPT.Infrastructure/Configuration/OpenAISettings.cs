// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Configuration
{
    public sealed class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string DefaultModel { get; set; } = "gpt-4o";

        public int MaxTokens { get; set; } = 4000;
    }
}
