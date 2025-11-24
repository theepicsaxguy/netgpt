// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Application.DTOs
{
    public record SendMessageRequest(
        string Content,
        List<FileAttachmentDto>? Attachments = null);
}
