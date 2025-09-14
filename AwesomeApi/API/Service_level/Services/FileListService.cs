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
        _filesRoot = configuration["Api-settings:filesRoot"]
                     ?? Environment.GetEnvironmentVariable("FILES_ROOT")
                     ?? Path.Combine(Directory.GetCurrentDirectory(), "files");

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
            throw new Exception(directoryInfo.FullName);
            _logger.LogError($"filepath{directoryInfo}");
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

        // Normalize path separators to forward slashes
        filePath = filePath.Replace('\\', '/');

        // Check if any part of the path is empty (double slashes, etc.)
        var pathParts = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Any(string.IsNullOrWhiteSpace))
            return false;

        return true;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (!IsValidFilePath(filePath))
            return false;

        // Normalize path separators
        var normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_filesRoot, normalizedPath);

        // Additional security check to ensure the resolved path is still within the files root
        var resolvedPath = Path.GetFullPath(fullPath);
        var resolvedRoot = Path.GetFullPath(_filesRoot);

        if (!resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt detected: {FilePath}", filePath);
            return false;
        }

        return await Task.FromResult(File.Exists(fullPath));
    }
}
