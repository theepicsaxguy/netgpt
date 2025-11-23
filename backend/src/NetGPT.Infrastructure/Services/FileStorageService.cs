using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetGPT.Application.Interfaces;

namespace NetGPT.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public FileStorageService(string storagePath)
    {
        _storagePath = storagePath;
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var storageKey = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, storageKey);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, ct);

        return storageKey;
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_storagePath, storageKey);
        return File.OpenRead(filePath);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_storagePath, storageKey);
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