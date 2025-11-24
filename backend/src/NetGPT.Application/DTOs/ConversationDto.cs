// Copyright (c) 2025 NetGPT. All rights reserved.

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
