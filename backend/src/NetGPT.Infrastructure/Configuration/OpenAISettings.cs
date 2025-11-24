// <copyright file="OpenAISettings.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Infrastructure.Configuration
{
    public sealed class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string DefaultModel { get; set; } = "gpt-4o";

        public int MaxTokens { get; set; } = 4000;
    }
}
