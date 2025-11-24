// <copyright file="ConversationDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public record ConversationDto(
        Guid Id,
        string Title,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int MessageCount);
}
