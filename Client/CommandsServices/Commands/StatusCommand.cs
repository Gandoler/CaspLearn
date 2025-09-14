using System.CommandLine;
using AwesomeFiles.Client.Models;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public class StatusCommand
{
    public static Command CreateCommand(IServiceProvider serviceProvider)
    {
        var idArgument = new Argument<string>("id", "Archive task ID");
        
        var command = new Command("status", "Get status of an archive task")
        {
            idArgument
        };
        
        command.SetHandler(async (string id) =>
        {
            try
            {
                if (!Guid.TryParse(id, out var archiveId))
                {
                    Console.WriteLine("Error: Invalid archive ID format");
                    Environment.Exit(1);
                    return;
                }
                
                var apiClient = serviceProvider.GetRequiredService<ApiClient>();
                var status = await apiClient.GetArchiveStatusAsync(archiveId);
                
                switch (status.Status)
                {
                    case ArchiveStatus.Pending:
                        Console.WriteLine("Archive is pending...");
                        break;
                    case ArchiveStatus.Processing:
                        Console.WriteLine($"Process in progress, please wait… ({status.Progress}%)");
                        break;
                    case ArchiveStatus.Ready:
                        Console.WriteLine("Archive has been created.");
                        break;
                    case Models.ArchiveStatus.Failed:
                        Console.WriteLine($"Archive creation failed: {status.Message ?? "Unknown error"}");
                        break;
                    default:
                        Console.WriteLine($"Unknown status: {status.Status}");
                        break;
                }
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
        }, idArgument);
        
        return command;
    }
}
