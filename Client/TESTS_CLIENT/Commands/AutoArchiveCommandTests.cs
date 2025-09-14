using System.CommandLine;
using AwesomeFiles.Client.Commands;
using AwesomeFiles.Client.Models;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests_client.Commands;

public class AutoArchiveCommandTests
{
    [Fact]
    public void AutoArchiveCommand_ShouldBeCreated()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        
        // Act
        var command = AutoArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal("auto-archive", command.Name);
        Assert.Equal("Automatically create archive, wait for completion, and download", command.Description);
    }

    [Fact]
    public async Task AutoArchiveCommand_ShouldHandleApiException()
    {
        // Arrange
        var mockApiClient = new Mock<ApiClient>(Mock.Of<HttpClient>(), Mock.Of<ILogger<ApiClient>>(), Mock.Of<IConfiguration>());
        mockApiClient.Setup(x => x.CreateArchiveAsync(It.IsAny<List<string>>()))
            .ThrowsAsync(new ApiException("Files not found"));
        
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<ApiClient>())
            .Returns(mockApiClient.Object);
        
        var command = AutoArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Act
        var result = await command.InvokeAsync("file1.txt file2.txt --output /tmp/test.zip");
        
        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task AutoArchiveCommand_ShouldHandleFailedStatus()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var mockApiClient = new Mock<ApiClient>(Mock.Of<HttpClient>(), Mock.Of<ILogger<ApiClient>>(), Mock.Of<IConfiguration>());
        
        mockApiClient.Setup(x => x.CreateArchiveAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(archiveId);
        
        var failedStatus = new ArchiveStatusResponse
        {
            Id = archiveId,
            Status = ArchiveStatus.Failed,
            Progress = 0,
            Message = "Archive creation failed"
        };
        
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ReturnsAsync(failedStatus);
        
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<ApiClient>())
            .Returns(mockApiClient.Object);
        
        var command = AutoArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Act
        var result = await command.InvokeAsync("file1.txt file2.txt --output /tmp/test.zip");
        
        // Assert
        Assert.Equal(1, result);
    }
}
