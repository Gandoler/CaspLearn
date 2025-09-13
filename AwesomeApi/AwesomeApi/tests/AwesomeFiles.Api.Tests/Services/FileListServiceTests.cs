using AwesomeFiles.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AwesomeFiles.Api.Tests.Services;

public class FileListServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<FileListService>> _loggerMock;

    public FileListServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FILES_ROOT"] = _tempDirectory
            })
            .Build();
            
        _loggerMock = new Mock<ILogger<FileListService>>();
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnEmptyList_WhenDirectoryIsEmpty()
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.GetFilesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnFiles_WhenFilesExist()
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);
        
        // Create test files
        var file1 = Path.Combine(_tempDirectory, "test1.txt");
        var file2 = Path.Combine(_tempDirectory, "subfolder", "test2.txt");
        Directory.CreateDirectory(Path.Combine(_tempDirectory, "subfolder"));
        
        await File.WriteAllTextAsync(file1, "content1");
        await File.WriteAllTextAsync(file2, "content2");

        // Act
        var result = await service.GetFilesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Name == "test1.txt");
        Assert.Contains(result, f => f.Name == "subfolder/test2.txt");
    }

    [Theory]
    [InlineData("valid-file.txt", true)]
    [InlineData("subfolder/file.txt", true)]
    [InlineData("../secret.txt", false)]
    [InlineData("..\\secret.txt", false)]
    [InlineData("/absolute/path.txt", false)]
    [InlineData("C:\\absolute\\path.txt", false)]
    [InlineData("file|with|pipes.txt", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidFilePath_ShouldValidateCorrectly(string filePath, bool expected)
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);

        // Act
        var result = service.IsValidFilePath(filePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);
        var testFile = Path.Combine(_tempDirectory, "existing.txt");
        await File.WriteAllTextAsync(testFile, "content");

        // Act
        var result = await service.FileExistsAsync("existing.txt");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.FileExistsAsync("nonexistent.txt");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenPathIsInvalid()
    {
        // Arrange
        var service = new FileListService(_configuration, _loggerMock.Object);

        // Act
        var result = await service.FileExistsAsync("../secret.txt");

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}
