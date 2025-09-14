
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

public class ListCommandTests
{
    [Fact]
    public void ListCommand_ShouldBeCreated()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IApiClient>());
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var command = ListCommand.CreateCommand(serviceProvider);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal("list", command.Name);
        Assert.Equal("List all available files", command.Description);
    }

    [Fact]
    public async Task ListCommand_ShouldHandleEmptyFileList()
    {
        // Arrange
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetFilesAsync())
            .ReturnsAsync(new List<FileMetadata>());
        
        var services = new ServiceCollection();
        services.AddSingleton(mockApiClient.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var command = ListCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync("");
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ListCommand_ShouldHandleFileList()
    {
        // Arrange
        var files = new List<FileMetadata>
        {
            new() { Name = "file1.txt", Size = 1024, Modified = DateTime.UtcNow },
            new() { Name = "file2.txt", Size = 2048, Modified = DateTime.UtcNow }
        };
        
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetFilesAsync())
            .ReturnsAsync(files);
        
        var services = new ServiceCollection();
        services.AddSingleton(mockApiClient.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var command = ListCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync("");
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ListCommand_ShouldHandleApiException()
    {
        // Arrange
        var mockApiClient = new Mock<IApiClient>();
        mockApiClient.Setup(x => x.GetFilesAsync())
            .ThrowsAsync(new ApiException("API Error"));
        
        var services = new ServiceCollection();
        services.AddSingleton(mockApiClient.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var command = ListCommand.CreateCommand(serviceProvider);
        
        // Act
        var result = await command.InvokeAsync("");
        
        // Assert
        Assert.Equal(1, result);
    }
}
