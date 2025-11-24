// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record ErrorResponse(
        string Code,
        string Message,
        Dictionary<string, string[]>? ValidationErrors = null);
}
