// <copyright file="ErrorResponse.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record ErrorResponse(
        string Code,
        string Message,
        Dictionary<string, string[]>? ValidationErrors = null);
}
