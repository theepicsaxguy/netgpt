
namespace NetGPT.Application.DTOs
{
    public record FileAttachmentDto(
        string FileName,
        string ContentType,
        long SizeBytes,
        string Base64Content);
}
