namespace NetGPT.Application.DTOs;

public record SendMessageRequest(
    string Content,
    List<Guid>? AttachmentIds = null);