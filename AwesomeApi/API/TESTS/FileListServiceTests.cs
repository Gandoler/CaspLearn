using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        // Конфигурация теперь соответствует ключу, который ищет сервис
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Api-settings:filesRoot"] = _tempDirectory
            })
            .Build();

        _loggerMock = new Mock<ILogger<FileListService>>();
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnEmptyList_WhenDirectoryIsEmpty()
    {
        var service = new FileListService(_configuration, _loggerMock.Object);

        var result = await service.GetFilesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnFiles_WhenFilesExist()
    {
        var service = new FileListService(_configuration, _loggerMock.Object);

        // Создаём тестовые файлы
        var file1 = Path.Combine(_tempDirectory, "test1.txt");
        var subfolder = Path.Combine(_tempDirectory, "subfolder");
        Directory.CreateDirectory(subfolder);
        var file2 = Path.Combine(subfolder, "test2.txt");

        await File.WriteAllTextAsync(file1, "content1");
        await File.WriteAllTextAsync(file2, "content2");

        var result = await service.GetFilesAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, f => f.Name == "test1.txt");
        Assert.Contains(result, f => f.Name == "subfolder/test2.txt");
    }

    
    public bool IsValidFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;
        if (filePath.Contains("..") || Path.IsPathRooted(filePath)) return false;

        filePath = filePath.Replace('\\', '/');
        var parts = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var invalidChars = Path.GetInvalidFileNameChars();

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) return false;
            if (part.IndexOfAny(invalidChars) >= 0) return false;
        }

        return true;
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        var service = new FileListService(_configuration, _loggerMock.Object);
        var testFile = Path.Combine(_tempDirectory, "existing.txt");
        await File.WriteAllTextAsync(testFile, "content");

        var result = await service.FileExistsAsync("existing.txt");

        Assert.True(result);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        var service = new FileListService(_configuration, _loggerMock.Object);

        var result = await service.FileExistsAsync("nonexistent.txt");

        Assert.False(result);
    }

    [Fact]
    public async Task FileExistsAsync_ShouldReturnFalse_WhenPathIsInvalid()
    {
        var service = new FileListService(_configuration, _loggerMock.Object);

        var result = await service.FileExistsAsync("../secret.txt");

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
