using System;

namespace NetGPT.Domain.ValueObjects;

public record Attachment(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey)
{
    public bool IsImage => ContentType.StartsWith("image/");
    public bool IsDocument => ContentType == "application/pdf" || 
                              ContentType.Contains("document") ||
                              ContentType.Contains("text");
}