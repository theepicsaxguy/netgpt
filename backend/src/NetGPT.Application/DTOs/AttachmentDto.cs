// <copyright file="AttachmentDto.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    public record AttachmentDto(
        string FileName,
        string ContentType,
        long SizeBytes,
        string Url);
}
