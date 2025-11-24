// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record SendMessageRequest(
        string Content,
        List<FileAttachmentDto>? Attachments = null);
}
