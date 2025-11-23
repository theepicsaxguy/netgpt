using System;
using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record MessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt,
    List<AttachmentDto>? Attachments = null,
    MessageMetadataDto? Metadata = null);
