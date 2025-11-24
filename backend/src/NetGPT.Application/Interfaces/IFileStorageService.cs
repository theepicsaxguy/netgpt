// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Interfaces
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

        Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default);

        Task DeleteAsync(string storageKey, CancellationToken ct = default);

        string GetPublicUrl(string storageKey);
    }
}
