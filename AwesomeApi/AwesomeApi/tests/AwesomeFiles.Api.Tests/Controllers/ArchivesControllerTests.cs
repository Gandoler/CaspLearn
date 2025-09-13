using AwesomeFiles.Api.Controllers;
using AwesomeFiles.Api.Services;
using AwesomeFiles.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AwesomeFiles.Api.Tests.Controllers;

public class ArchivesControllerTests
{
    private readonly Mock<IArchiveService> _archiveServiceMock;
    private readonly Mock<IFileListService> _fileListServiceMock;
    private readonly Mock<ILogger<ArchivesController>> _loggerMock;
    private readonly ArchivesController _controller;

    public ArchivesControllerTests()
    {
        _archiveServiceMock = new Mock<IArchiveService>();
        _fileListServiceMock = new Mock<IFileListService>();
        _loggerMock = new Mock<ILogger<ArchivesController>>();
        
        _controller = new ArchivesController(
            _archiveServiceMock.Object,
            _fileListServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateArchive_ShouldReturnAccepted_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateArchiveRequest
        {
            Files = new List<string> { "file1.txt", "file2.txt" }
        };
        
        var taskId = Guid.NewGuid();
        
        _fileListServiceMock.Setup(x => x.IsValidFilePath(It.IsAny<string>()))
            .Returns(true);
        _fileListServiceMock.Setup(x => x.FileExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _archiveServiceMock.Setup(x => x.CreateArchiveAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(taskId);

        // Act
        var result = await _controller.CreateArchive(request);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var response = Assert.IsType<CreateArchiveResponse>(acceptedResult.Value);
        Assert.Equal(taskId, response.Id);
    }

    [Fact]
    public async Task CreateArchive_ShouldReturnBadRequest_WhenFilesListIsEmpty()
    {
        // Arrange
        var request = new CreateArchiveRequest
        {
            Files = new List<string>()
        };

        // Act
        var result = await _controller.CreateArchive(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateArchive_ShouldReturnBadRequest_WhenFileDoesNotExist()
    {
        // Arrange
        var request = new CreateArchiveRequest
        {
            Files = new List<string> { "nonexistent.txt" }
        };
        
        _fileListServiceMock.Setup(x => x.IsValidFilePath(It.IsAny<string>()))
            .Returns(true);
        _fileListServiceMock.Setup(x => x.FileExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CreateArchive(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Files not found", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task CreateArchive_ShouldReturnBadRequest_WhenFilePathIsInvalid()
    {
        // Arrange
        var request = new CreateArchiveRequest
        {
            Files = new List<string> { "../secret.txt" }
        };
        
        _fileListServiceMock.Setup(x => x.IsValidFilePath(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _controller.CreateArchive(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid file paths", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task GetArchiveStatus_ShouldReturnOk_WhenTaskExists()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AwesomeFiles.Common.Models.ArchiveTask
        {
            Id = taskId,
            Status = AwesomeFiles.Common.Models.ArchiveStatus.Processing,
            Progress = 50,
            Message = "Processing files"
        };
        
        _archiveServiceMock.Setup(x => x.GetTaskAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetArchiveStatus(taskId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ArchiveStatusResponse>(okResult.Value);
        Assert.Equal(taskId, response.Id);
        Assert.Equal(task.Status, response.Status);
        Assert.Equal(task.Progress, response.Progress);
        Assert.Equal(task.Message, response.Message);
    }

    [Fact]
    public async Task GetArchiveStatus_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        
        _archiveServiceMock.Setup(x => x.GetTaskAsync(taskId))
            .ReturnsAsync((AwesomeFiles.Common.Models.ArchiveTask?)null);

        // Act
        var result = await _controller.GetArchiveStatus(taskId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DownloadArchive_ShouldReturnFileStream_WhenArchiveIsReady()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AwesomeFiles.Common.Models.ArchiveTask
        {
            Id = taskId,
            Status = AwesomeFiles.Common.Models.ArchiveStatus.Ready,
            FilePath = "/path/to/archive.zip"
        };
        
        _archiveServiceMock.Setup(x => x.GetTaskAsync(taskId))
            .ReturnsAsync(task);
        _archiveServiceMock.Setup(x => x.GetArchiveFilePathAsync(taskId))
            .ReturnsAsync("/path/to/archive.zip");

        // Act
        var result = await _controller.DownloadArchive(taskId);

        // Assert
        Assert.IsType<FileStreamResult>(result);
    }

    [Fact]
    public async Task DownloadArchive_ShouldReturnConflict_WhenArchiveIsNotReady()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AwesomeFiles.Common.Models.ArchiveTask
        {
            Id = taskId,
            Status = AwesomeFiles.Common.Models.ArchiveStatus.Processing
        };
        
        _archiveServiceMock.Setup(x => x.GetTaskAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.DownloadArchive(taskId);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("Archive is not ready", conflictResult.Value?.ToString());
    }
}
