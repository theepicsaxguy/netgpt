// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System;
    using System.Collections.Generic;
    using NetGPT.Domain.Enums;

    public record MessageDto(
        Guid Id,
        MessageRole Role,
        string Content,
        DateTime CreatedAt,
        List<AttachmentDto>? Attachments = null,
        MessageMetadataDto? Metadata = null);
}
