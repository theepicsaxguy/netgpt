// <copyright file="Attachment.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Domain.ValueObjects
{
    public record Attachment(
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey)
    {
        public bool IsImage => this.ContentType.StartsWith("image/");

        public bool IsDocument => this.ContentType == "application/pdf" ||
                                  this.ContentType.Contains("document") ||
                                  this.ContentType.Contains("text");
    }
}
