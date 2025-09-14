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

public class StatusCommandTests
{
    [Fact]
    public void StatusCommand_ShouldBeCreated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal("status", command.Name);
        Assert.Equal("Get status of an archive task", command.Description);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandleInvalidGuid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync("invalid-guid");
        
        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandlePendingStatus()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var statusResponse = new ArchiveStatusResponse
        {
            Id = archiveId,
            Status = ArchiveStatus.Pending,
            Progress = 0,
            Message = null
        };
        
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ReturnsAsync(statusResponse);
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync(archiveId.ToString());
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandleProcessingStatus()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var statusResponse = new ArchiveStatusResponse
        {
            Id = archiveId,
            Status = ArchiveStatus.Processing,
            Progress = 50,
            Message = "Creating archive..."
        };
        
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ReturnsAsync(statusResponse);
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync(archiveId.ToString());
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandleReadyStatus()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var statusResponse = new ArchiveStatusResponse
        {
            Id = archiveId,
            Status = ArchiveStatus.Ready,
            Progress = 100,
            Message = "Archive completed"
        };
        
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ReturnsAsync(statusResponse);
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync(archiveId.ToString());
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandleFailedStatus()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var statusResponse = new ArchiveStatusResponse
        {
            Id = archiveId,
            Status = ArchiveStatus.Failed,
            Progress = 0,
            Message = "Archive creation failed"
        };
        
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ReturnsAsync(statusResponse);
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync(archiveId.ToString());
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task StatusCommand_ShouldHandleApiException()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetArchiveStatusAsync(archiveId))
            .ThrowsAsync(new ApiException("Archive not found"));
        
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        var command = StatusCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync(archiveId.ToString());
        
        // Assert
        Assert.Equal(1, result);
    }
}
