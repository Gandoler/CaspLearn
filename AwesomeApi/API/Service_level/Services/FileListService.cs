using AwesomeFiles.Common.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AwesomeFiles.Api.Services;

public class FileListService : IFileListService
{
    private readonly string _filesRoot;
    private readonly ILogger<FileListService> _logger;

    public FileListService(IConfiguration configuration, ILogger<FileListService> logger)
    {
        _filesRoot = configuration["FILES_ROOT"] ?? Path.Combine(Directory.GetCurrentDirectory(), "files");
        _logger = logger;
        
        // Ensure the files directory exists
        Directory.CreateDirectory(_filesRoot);
    }

    public async Task<List<FileMetadata>> GetFilesAsync()
    {
        try
        {
            var files = new List<FileMetadata>();
            var directoryInfo = new DirectoryInfo(_filesRoot);
            
            await Task.Run(() =>
            {
                foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(_filesRoot, file.FullName);
                    files.Add(new FileMetadata
                    {
                        Name = relativePath.Replace('\\', '/'), // Normalize path separators
                        Size = file.Length,
                        Modified = file.LastWriteTimeUtc
                    });
                }
            });

            _logger.LogInformation("Retrieved {Count} files from {FilesRoot}", files.Count, _filesRoot);
            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files from {FilesRoot}", _filesRoot);
            throw;
        }
    }

    public bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Prevent path traversal attacks
        if (filePath.Contains("..") || Path.IsPathRooted(filePath))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars());
        if (filePath.IndexOfAny(invalidChars.ToArray()) >= 0)
            return false;

        return true;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (!IsValidFilePath(filePath))
            return false;

        var fullPath = Path.Combine(_filesRoot, filePath);
        return await Task.FromResult(File.Exists(fullPath));
    }
}
