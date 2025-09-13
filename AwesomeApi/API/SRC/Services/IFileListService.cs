using AwesomeFiles.Common.Models;

namespace AwesomeFiles.Api.Services;

public interface IFileListService
{
    Task<List<FileMetadata>> GetFilesAsync();
    bool IsValidFilePath(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}
