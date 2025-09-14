using AwesomeFiles.Common.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AwesomeFiles.Api.Services;

public class ArchiveService : IArchiveService
{
    private readonly ConcurrentDictionary<Guid, ArchiveTask> _tasks = new();
    private readonly Channel<ArchiveTask> _taskQueue;
    private readonly ILogger<ArchiveService> _logger;
    private readonly IFileListService _fileListService;
    private readonly string _archivesDir;

    public ArchiveService(
        IConfiguration configuration,
        ILogger<ArchiveService> logger,
        IFileListService fileListService)
    {
        _logger = logger;
        _fileListService = fileListService;
        _archivesDir = configuration["Api-settings:ArchivesDir"] 
                       ?? Environment.GetEnvironmentVariable("ARCHIVES_DIR") 
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "files");
            
        
        // Ensure the archives directory exists
        Directory.CreateDirectory(_archivesDir);
        
        // Create bounded channel for task queue
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        _taskQueue = Channel.CreateBounded<ArchiveTask>(options);
    }

    public async Task<Guid> CreateArchiveAsync(List<string> files)
    {
        var taskId = Guid.NewGuid();
        var task = new ArchiveTask
        {
            Id = taskId,
            Status = ArchiveStatus.Pending,
            Files = files,
            CreatedAt = DateTime.UtcNow
        };

        _tasks[taskId] = task;
        
        // Validate all files exist
        foreach (var file in files)
        {
            if (!await _fileListService.FileExistsAsync(file))
            {
                task.Status = ArchiveStatus.Failed;
                task.Message = $"File not found: {file}";
                _logger.LogWarning("Archive task {TaskId} failed: file {File} not found", taskId, file);
                return taskId;
            }
        }

        // Queue the task for processing
        await _taskQueue.Writer.WriteAsync(task);
        _logger.LogInformation("Created archive task {TaskId} with {FileCount} files", taskId, files.Count);
        
        return taskId;
    }

    public Task<ArchiveTask?> GetTaskAsync(Guid id)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<string?> GetArchiveFilePathAsync(Guid id)
    {
        if (_tasks.TryGetValue(id, out var task) && task.Status == ArchiveStatus.Ready)
        {
            return Task.FromResult(task.FilePath);
        }
        return Task.FromResult<string?>(null);
    }

    public Task<bool> DeleteArchiveAsync(Guid id)
    {
        if (_tasks.TryRemove(id, out var task) && !string.IsNullOrEmpty(task.FilePath))
        {
            try
            {
                if (File.Exists(task.FilePath))
                {
                    File.Delete(task.FilePath);
                    _logger.LogInformation("Deleted archive file {FilePath} for task {TaskId}", task.FilePath, id);
                }
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting archive file {FilePath} for task {TaskId}", task.FilePath, id);
            }
        }
        return Task.FromResult(false);
    }

    public ChannelReader<ArchiveTask> GetTaskReader() => _taskQueue.Reader;

    public void UpdateTaskStatus(Guid id, ArchiveStatus status, int progress = 0, string? message = null)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.Status = status;
            task.Progress = progress;
            task.Message = message;
            
            if (status == ArchiveStatus.Ready || status == ArchiveStatus.Failed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            
            _logger.LogDebug("Updated task {TaskId} status to {Status} with progress {Progress}", id, status, progress);
        }
    }

    public void SetTaskFilePath(Guid id, string filePath)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.FilePath = filePath;
            _logger.LogDebug("Set file path {FilePath} for task {TaskId}", filePath, id);
        }
    }
}
