// <copyright file="ToolInvocation.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.ValueObjects
{
    using System;

    public record ToolInvocation(
        string ToolName,
        string Arguments,
        string? Result,
        DateTime InvokedAt,
        TimeSpan Duration);
}
