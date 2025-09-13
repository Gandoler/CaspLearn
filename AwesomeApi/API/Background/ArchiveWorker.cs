using AwesomeFiles.Api.Services;
using AwesomeFiles.Common.Models;
using System.IO.Compression;
using System.Threading.Channels;

namespace AwesomeFiles.Api.Background;

public class ArchiveWorker : BackgroundService
{
    private readonly ArchiveService _archiveService;
    private readonly IFileListService _fileListService;
    private readonly ILogger<ArchiveWorker> _logger;
    private readonly string _filesRoot;
    private readonly string _archivesDir;

    public ArchiveWorker(
        ArchiveService archiveService,
        IFileListService fileListService,
        IConfiguration configuration,
        ILogger<ArchiveWorker> logger)
    {
        _archiveService = archiveService;
        _fileListService = fileListService;
        _logger = logger;
        _filesRoot = configuration["FILES_ROOT"] ?? Path.Combine(Directory.GetCurrentDirectory(), "files");
        _archivesDir = configuration["ARCHIVES_DIR"] ?? Path.Combine(Directory.GetCurrentDirectory(), "archives");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Archive worker started");

        var reader = _archiveService.GetTaskReader();
        
        await foreach (var task in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessArchiveTaskAsync(task, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing archive task {TaskId}", task.Id);
                _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Failed, 0, ex.Message);
            }
        }

        _logger.LogInformation("Archive worker stopped");
    }

    private async Task ProcessArchiveTaskAsync(ArchiveTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing archive task {TaskId} with {FileCount} files", task.Id, task.Files.Count);
        
        _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Processing, 0, "Starting archive creation");

        var tempFilePath = Path.Combine(_archivesDir, $"archive-{task.Id}.zip.tmp");
        var finalFilePath = Path.Combine(_archivesDir, $"archive-{task.Id}.zip");

        try
        {
            // Check if archive already exists (cache hit)
            if (File.Exists(finalFilePath))
            {
                _logger.LogInformation("Archive {TaskId} already exists, using cached version", task.Id);
                _archiveService.SetTaskFilePath(task.Id, finalFilePath);
                _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Ready, 100, "Archive ready (cached)");
                return;
            }

            await CreateZipArchiveAsync(task, tempFilePath, cancellationToken);

            // Atomically move temp file to final location
            File.Move(tempFilePath, finalFilePath);
            
            _archiveService.SetTaskFilePath(task.Id, finalFilePath);
            _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Ready, 100, "Archive ready");
            
            _logger.LogInformation("Successfully created archive {TaskId} at {FilePath}", task.Id, finalFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create archive {TaskId}", task.Id);
            _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Failed, 0, ex.Message);
            
            // Clean up temp file if it exists
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to delete temp file {TempFilePath}", tempFilePath);
                }
            }
        }
    }

    private async Task CreateZipArchiveAsync(ArchiveTask task, string zipFilePath, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

        var totalFiles = task.Files.Count;
        var processedFiles = 0;

        foreach (var relativeFilePath in task.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullFilePath = Path.Combine(_filesRoot, relativeFilePath);
            
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"File not found: {relativeFilePath}");
            }

            // Create entry in zip archive
            var entry = archive.CreateEntry(relativeFilePath.Replace('\\', '/'));
            
            // Copy file content to zip entry
            using var entryStream = entry.Open();
            using var sourceStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            
            await sourceStream.CopyToAsync(entryStream, cancellationToken);
            
            processedFiles++;
            var progress = (int)((double)processedFiles / totalFiles * 100);
            _archiveService.UpdateTaskStatus(task.Id, ArchiveStatus.Processing, progress, $"Processing file {processedFiles}/{totalFiles}");
            
            _logger.LogDebug("Added file {FilePath} to archive {TaskId} ({Progress}%)", relativeFilePath, task.Id, progress);
        }
    }
}
