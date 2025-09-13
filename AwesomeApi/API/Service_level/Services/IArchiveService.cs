using AwesomeFiles.Common.Models;

namespace AwesomeFiles.Api.Services;

public interface IArchiveService
{
    Task<Guid> CreateArchiveAsync(List<string> files);
    Task<ArchiveTask?> GetTaskAsync(Guid id);
    Task<string?> GetArchiveFilePathAsync(Guid id);
    Task<bool> DeleteArchiveAsync(Guid id);
}
