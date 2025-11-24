
namespace NetGPT.Domain.ValueObjects
{
    using System.Collections.Generic;

    public record MessageMetadata(
        List<ToolInvocation>? ToolInvocations = null,
        string? AgentName = null,
        int? TokenCount = null,
        Dictionary<string, object>? CustomProperties = null);
}
