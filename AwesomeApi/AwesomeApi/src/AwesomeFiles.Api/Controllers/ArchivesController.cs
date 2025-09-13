using AwesomeFiles.Api.Services;
using AwesomeFiles.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AwesomeFiles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchivesController : ControllerBase
{
    private readonly IArchiveService _archiveService;
    private readonly IFileListService _fileListService;
    private readonly ILogger<ArchivesController> _logger;

    public ArchivesController(
        IArchiveService archiveService,
        IFileListService fileListService,
        ILogger<ArchivesController> logger)
    {
        _archiveService = archiveService;
        _fileListService = fileListService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new archive from specified files
    /// </summary>
    /// <param name="request">List of files to include in archive</param>
    /// <returns>Archive task ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateArchiveResponse), 202)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateArchive([FromBody] CreateArchiveRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validate all file paths
            var invalidFiles = new List<string>();
            foreach (var file in request.Files)
            {
                if (!_fileListService.IsValidFilePath(file))
                {
                    invalidFiles.Add(file);
                }
            }

            if (invalidFiles.Any())
            {
                return BadRequest(new { error = "Invalid file paths", files = invalidFiles });
            }

            // Check if all files exist
            var missingFiles = new List<string>();
            foreach (var file in request.Files)
            {
                if (!await _fileListService.FileExistsAsync(file))
                {
                    missingFiles.Add(file);
                }
            }

            if (missingFiles.Any())
            {
                return BadRequest(new { error = "Files not found", files = missingFiles });
            }

            var taskId = await _archiveService.CreateArchiveAsync(request.Files);
            
            _logger.LogInformation("Created archive task {TaskId} for {FileCount} files", taskId, request.Files.Count);
            
            return Accepted(new CreateArchiveResponse { Id = taskId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating archive");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get status of an archive task
    /// </summary>
    /// <param name="id">Archive task ID</param>
    /// <returns>Task status and progress</returns>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(ArchiveStatusResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetArchiveStatus(Guid id)
    {
        try
        {
            var task = await _archiveService.GetTaskAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Archive task not found" });
            }

            var response = new ArchiveStatusResponse
            {
                Id = task.Id,
                Status = task.Status,
                Progress = task.Progress,
                Message = task.Message
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving archive status for {TaskId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Download a completed archive
    /// </summary>
    /// <param name="id">Archive task ID</param>
    /// <returns>ZIP file stream</returns>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DownloadArchive(Guid id)
    {
        try
        {
            var task = await _archiveService.GetTaskAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Archive task not found" });
            }

            if (task.Status != AwesomeFiles.Common.Models.ArchiveStatus.Ready)
            {
                return Conflict(new { error = "Archive is not ready", status = task.Status.ToString() });
            }

            var filePath = await _archiveService.GetArchiveFilePathAsync(id);
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound(new { error = "Archive file not found" });
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileName = $"archive-{id}.zip";
            
            _logger.LogInformation("Downloading archive {TaskId} as {FileName}", id, fileName);
            
            return File(fileStream, "application/zip", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading archive {TaskId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
