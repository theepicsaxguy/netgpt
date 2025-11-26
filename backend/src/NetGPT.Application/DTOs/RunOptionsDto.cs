// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Options for configuring agent run behavior.
    /// </summary>
    public record RunOptionsDto(
        string? ModelOverride = null,
        int? MaxTokens = null,
        double? Temperature = null,
        List<string>? EnabledTools = null,
        List<string>? DisabledTools = null);
}
