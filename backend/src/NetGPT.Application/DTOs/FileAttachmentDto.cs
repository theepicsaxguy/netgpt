// <copyright file="FileAttachmentDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    public record FileAttachmentDto(
        string FileName,
        string ContentType,
        long SizeBytes,
        string Base64Content);
}
