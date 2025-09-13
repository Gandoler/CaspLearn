using AwesomeFiles.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AwesomeFiles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileListService _fileListService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileListService fileListService, ILogger<FilesController> logger)
    {
        _fileListService = fileListService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all available files
    /// </summary>
    /// <returns>List of files with metadata</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AwesomeFiles.Common.Models.FileMetadata>), 200)]
    public async Task<IActionResult> GetFiles()
    {
        try
        {
            var files = await _fileListService.GetFilesAsync();
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files");
            return StatusCode(500, "Internal server error");
        }
    }
}
