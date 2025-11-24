// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.DTOs
{
    public record AttachmentDto(
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
}
