// <copyright file="SendMessageRequest.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record SendMessageRequest(
        string Content,
        List<FileAttachmentDto>? Attachments = null);
}
