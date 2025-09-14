using System.CommandLine;
using AwesomeFiles.Client.Commands;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests_client.Commands;

public class CreateArchiveCommandTests
{
    [Fact]
    public void CreateArchiveCommand_ShouldBeCreated()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        
        // Act
        var command = CreateArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal("create-archive", command.Name);
        Assert.Equal("Create an archive from specified files", command.Description);
    }

    [Fact]
    public async Task CreateArchiveCommand_ShouldHandleSuccess()
    {
        // Arrange
        var archiveId = Guid.NewGuid();
        var mockApiClient = new Mock<ApiClient>(Mock.Of<HttpClient>(), Mock.Of<ILogger<ApiClient>>(), Mock.Of<IConfiguration>());
        mockApiClient.Setup(x => x.CreateArchiveAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(archiveId);
        
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<ApiClient>())
            .Returns(mockApiClient.Object);
        
        var command = CreateArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Act
        var result = await command.InvokeAsync("file1.txt file2.txt");
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CreateArchiveCommand_ShouldHandleApiException()
    {
        // Arrange
        var mockApiClient = new Mock<ApiClient>(Mock.Of<HttpClient>(), Mock.Of<ILogger<ApiClient>>(), Mock.Of<IConfiguration>());
        mockApiClient.Setup(x => x.CreateArchiveAsync(It.IsAny<List<string>>()))
            .ThrowsAsync(new ApiException("Files not found"));
        
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetRequiredService<ApiClient>())
            .Returns(mockApiClient.Object);
        
        var command = CreateArchiveCommand.CreateCommand(serviceProvider.Object);
        
        // Act
        var result = await command.InvokeAsync("file1.txt file2.txt");
        
        // Assert
        Assert.Equal(1, result);
    }
}
