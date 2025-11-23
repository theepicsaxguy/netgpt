using NetGPT.Domain.Enums;

namespace NetGPT.Application.DTOs;

public record MessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    DateTime CreatedAt,
    List<AttachmentDto>? Attachments = null,
    MessageMetadataDto? Metadata = null);