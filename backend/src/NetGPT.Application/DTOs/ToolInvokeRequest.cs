// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    /// <summary>
    /// Request to invoke a tool.
    /// </summary>
    public record ToolInvokeRequest(
        Dictionary<string, object> Arguments);
}
