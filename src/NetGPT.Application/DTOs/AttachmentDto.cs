namespace NetGPT.Application.DTOs;

public record AttachmentDto(
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url);