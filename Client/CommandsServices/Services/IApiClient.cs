using AwesomeFiles.Client.Models;

namespace AwesomeFiles.Client.Services;

public interface IApiClient
{
    Task<List<FileMetadata>> GetFilesAsync();
    Task<Guid> CreateArchiveAsync(List<string> files);
    Task<ArchiveStatusResponse> GetArchiveStatusAsync(Guid archiveId);
    Task<Stream> DownloadArchiveAsync(Guid archiveId);
}

