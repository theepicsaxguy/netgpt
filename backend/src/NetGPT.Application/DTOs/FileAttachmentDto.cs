// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    public record FileAttachmentDto(
        string Url,
        string Name,
        int Size,
        string ContentType);
}
