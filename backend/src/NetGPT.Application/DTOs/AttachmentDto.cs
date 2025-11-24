// <copyright file="AttachmentDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    public record AttachmentDto(
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
}
