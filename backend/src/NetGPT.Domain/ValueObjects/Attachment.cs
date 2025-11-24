// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.ValueObjects
{
    public record Attachment(
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey)
    {
        public bool IsImage => ContentType.StartsWith("image/", StringComparison.Ordinal);

        public bool IsDocument => ContentType == "application/pdf" ||
                                  ContentType.Contains("document") ||
                                  ContentType.Contains("text");
    }
}
