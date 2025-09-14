using System.CommandLine;
using AwesomeFiles.Client.Commands;
using AwesomeFiles.Client.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests_client.Commands;

public class DownloadCommandTests
{
    [Fact]
    public void DownloadCommand_ShouldBeCreated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var command = DownloadCommand.CreateCommand(serviceProvider);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal("download", command.Name);
        Assert.Equal("Download a completed archive", command.Description);
    }

    [Fact]
    public async Task DownloadCommand_ShouldHandleInvalidGuid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        var command = DownloadCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync("invalid-guid /tmp/test.zip");
        
        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task DownloadCommand_ShouldHandleApiException()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.DownloadArchiveAsync(archiveId))
            .ThrowsAsync(new ApiException("Archive not found"));
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = DownloadCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync($"{archiveId} /tmp/test.zip");
        
        // Assert
        Assert.Equal(1, result);
    }
}
