// <copyright file="MessageResponse.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public record MessageResponse(
        Guid Id,
        string Role,
        string Content,
        DateTime CreatedAt,
        List<AttachmentDto>? Attachments = null,
        MessageMetadataDto? Metadata = null);
}
