using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record SendMessageRequest(
    string Content,
    List<FileAttachmentDto>? Attachments = null);