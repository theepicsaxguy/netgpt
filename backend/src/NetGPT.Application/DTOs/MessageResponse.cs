// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public record MessageResponse(
        Guid Id,
        string Role,
        string Content,
        DateTime CreatedAt,
        List<AttachmentDto>? Attachments = null,
        MessageMetadataDto? Metadata = null);
}
