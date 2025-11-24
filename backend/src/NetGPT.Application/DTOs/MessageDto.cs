// <copyright file="MessageDto.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

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
