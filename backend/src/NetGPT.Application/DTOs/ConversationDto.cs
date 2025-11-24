// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record ConversationDto(
        Guid Id,
        string Title,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int MessageCount);
}
