using LegalDocumentComparator.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace LegalDocumentComparator.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly string _storagePath;

    public StorageService(IConfiguration configuration)
    {
        _storagePath = configuration["Storage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage", "documents");
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using var fileStreamOutput = File.Create(filePath);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        return filePath;
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var memoryStream = new MemoryStream();
        using var fileStream = File.OpenRead(filePath);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(filePath));
    }
}
