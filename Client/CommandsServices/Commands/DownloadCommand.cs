using System.CommandLine;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public class DownloadCommand
{
    public static Command CreateCommand(IServiceProvider serviceProvider)
    {
        var idArgument = new Argument<string>("id", "Archive task ID");
        var pathArgument = new Argument<string>("path", "Path to save the downloaded archive");
        
        var command = new Command("download", "Download a completed archive")
        {
            idArgument,
            pathArgument
        };
        
        command.SetHandler(async (string id, string path) =>
        {
            try
            {
                if (!Guid.TryParse(id, out var archiveId))
                {
                    Console.WriteLine("Error: Invalid archive ID format");
                    Environment.Exit(1);
                    return;
                }
                
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var apiClient = serviceProvider.GetRequiredService<ApiClient>();
                
                using var stream = await apiClient.DownloadArchiveAsync(archiveId);
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                
                await stream.CopyToAsync(fileStream);
                
                Console.WriteLine($"Archive downloaded successfully to: {path}");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        }, idArgument, pathArgument);
        
        return command;
    }
}
