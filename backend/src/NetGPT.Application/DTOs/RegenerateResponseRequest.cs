// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Application.DTOs
{
    public record RegenerateResponseRequest(
        Guid ConversationId,
        Guid MessageId);
}
