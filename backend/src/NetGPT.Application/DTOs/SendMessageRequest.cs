// <copyright file="SendMessageRequest.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System.Collections.Generic;

    public record SendMessageRequest(
        string Content,
        List<FileAttachmentDto>? Attachments = null);
}
