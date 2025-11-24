// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using NetGPT.Domain.Enums;

namespace NetGPT.Application.DTOs
{
    public record MessageDto(
        Guid Id,
        MessageRole Role,
        string Content,
        DateTime CreatedAt,
        List<AttachmentDto>? Attachments = null,
        MessageMetadataDto? Metadata = null);
}
