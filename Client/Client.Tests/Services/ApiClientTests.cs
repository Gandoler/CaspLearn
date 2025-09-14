using System.Net;
using System.Text;
using AwesomeFiles.Client.Models;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AwesomeFiles.Client.Tests.Services;

public class ApiClientTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<ApiClient>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ApiClient _apiClient;

    public ApiClientTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<ApiClient>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["ApiSettings:BaseUrl"])
            .Returns("http://localhost:5010");
        
        _apiClient = new ApiClient(_mockHttpClient.Object, _mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnFiles_WhenApiReturnsSuccess()
    {
        // Arrange
        var expectedFiles = new List<FileMetadata>
        {
            new() { Name = "file1.txt", Size = 1024, Modified = DateTime.UtcNow },
            new() { Name = "file2.txt", Size = 2048, Modified = DateTime.UtcNow }
        };
        
        var json = JsonConvert.SerializeObject(expectedFiles);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        
        _mockHttpClient.Setup(x => x.GetAsync("http://localhost:5010/api/files"))
            .ReturnsAsync(response);

        // Act
        var result = await _apiClient.GetFilesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("file1.txt", result[0].Name);
        Assert.Equal(1024, result[0].Size);
        Assert.Equal("file2.txt", result[1].Name);
        Assert.Equal(2048, result[1].Size);
    }

    [Fact]
    public async Task GetFilesAsync_ShouldThrowApiException_WhenApiReturnsError()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":\"Invalid request\"}", Encoding.UTF8, "application/json")
        };
        
        _mockHttpClient.Setup(x => x.GetAsync("http://localhost:5010/api/files"))
            .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _apiClient.GetFilesAsync());
        Assert.Contains("Failed to get files", exception.Message);
    }

    [Fact]
    public async Task CreateArchiveAsync_ShouldReturnArchiveId_WhenApiReturnsSuccess()
    {
        // Arrange
        var files = new List<string> { "file1.txt", "file2.txt" };
        var expectedResponse = new { Id = Guid.NewGuid() };
        var json = JsonConvert.SerializeObject(expectedResponse);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = content };
        
        _mockHttpClient.Setup(x => x.PostAsync("http://localhost:5010/api/archives", It.IsAny<HttpContent>()))
            .ReturnsAsync(response);

        // Act
        var result = await _apiClient.CreateArchiveAsync(files);

        // Assert
        Assert.Equal(expectedResponse.Id, result);
    }

    [Fact]
    public async Task CreateArchiveAsync_ShouldThrowApiException_WhenApiReturnsError()
    {
        // Arrange
        var files = new List<string> { "file1.txt", "file2.txt" };
        var errorResponse = new { error = "Files not found", files = new[] { "file1.txt" } };
        var json = JsonConvert.SerializeObject(errorResponse);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content };
        
        _mockHttpClient.Setup(x => x.PostAsync("http://localhost:5010/api/archives", It.IsAny<HttpContent>()))
            .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _apiClient.CreateArchiveAsync(files));
        Assert.Contains("Files not found", exception.Message);
    }

    [Fact]
    public async Task GetArchiveStatusAsync_ShouldReturnStatus_WhenApiReturnsSuccess()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var expectedStatus = new
        {
            Id = archiveId,
            Status = "Processing",
            Progress = 50,
            Message = "Creating archive..."
        };
        
        var json = JsonConvert.SerializeObject(expectedStatus);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        
        _mockHttpClient.Setup(x => x.GetAsync($"http://localhost:5010/api/archives/{archiveId}/status"))
            .ReturnsAsync(response);

        // Act
        var result = await _apiClient.GetArchiveStatusAsync(archiveId);

        // Assert
        Assert.Equal(archiveId, result.Id);
        Assert.Equal(ArchiveStatus.Processing, result.Status);
        Assert.Equal(50, result.Progress);
        Assert.Equal("Creating archive...", result.Message);
    }

    [Fact]
    public async Task GetArchiveStatusAsync_ShouldThrowApiException_WhenApiReturnsError()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var errorResponse = new { error = "Archive task not found" };
        var json = JsonConvert.SerializeObject(errorResponse);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.NotFound) { Content = content };
        
        _mockHttpClient.Setup(x => x.GetAsync($"http://localhost:5010/api/archives/{archiveId}/status"))
            .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _apiClient.GetArchiveStatusAsync(archiveId));
        Assert.Contains("Archive task not found", exception.Message);
    }

    [Fact]
    public async Task DownloadArchiveAsync_ShouldReturnStream_WhenApiReturnsSuccess()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var testData = Encoding.UTF8.GetBytes("test archive content");
        var content = new ByteArrayContent(testData);
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        
        _mockHttpClient.Setup(x => x.GetAsync($"http://localhost:5010/api/archives/{archiveId}/download"))
            .ReturnsAsync(response);

        // Act
        var result = await _apiClient.DownloadArchiveAsync(archiveId);

        // Assert
        Assert.NotNull(result);
        var buffer = new byte[testData.Length];
        await result.ReadAsync(buffer, 0, buffer.Length);
        Assert.Equal(testData, buffer);
    }

    [Fact]
    public async Task DownloadArchiveAsync_ShouldThrowApiException_WhenApiReturnsError()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var errorResponse = new { error = "Archive is not ready", status = "Processing" };
        var json = JsonConvert.SerializeObject(errorResponse);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = content };
        
        _mockHttpClient.Setup(x => x.GetAsync($"http://localhost:5010/api/archives/{archiveId}/download"))
            .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _apiClient.DownloadArchiveAsync(archiveId));
        Assert.Contains("Archive is not ready", exception.Message);
    }
}
