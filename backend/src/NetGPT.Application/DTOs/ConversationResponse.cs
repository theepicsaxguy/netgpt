// <copyright file="ConversationResponse.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public record ConversationResponse(
        Guid Id,
        string Title,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int MessageCount);
}
