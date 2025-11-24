// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Infrastructure.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NetGPT.Application.Interfaces;

    public class FileStorageService : IFileStorageService
    {
        private readonly string storagePath;

        public FileStorageService(string storagePath)
        {
            this.storagePath = storagePath;
            _ = Directory.CreateDirectory(this.storagePath);
        }

        public async Task<string> UploadAsync(
            Stream content,
            string fileName,
            string contentType,
            CancellationToken ct = default)
        {
            string storageKey = $"{Guid.NewGuid()}_{fileName}";
            string filePath = Path.Combine(this.storagePath, storageKey);

            await using FileStream fileStream = File.Create(filePath);
            await content.CopyToAsync(fileStream, ct);

            return storageKey;
        }

        public async Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
        {
            string filePath = Path.Combine(this.storagePath, storageKey);
            return File.OpenRead(filePath);
        }

        public Task DeleteAsync(string storageKey, CancellationToken ct = default)
        {
            string filePath = Path.Combine(this.storagePath, storageKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public string GetPublicUrl(string storageKey)
        {
            return $"/api/v1/files/{storageKey}";
        }
    }
}
