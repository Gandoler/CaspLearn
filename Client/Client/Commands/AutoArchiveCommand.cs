using System.CommandLine;
using AwesomeFiles.Client.Models;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public class AutoArchiveCommand
{
    public static Command CreateCommand(IServiceProvider serviceProvider)
    {
        var filesArgument = new Argument<string[]>("files", "List of files to include in the archive");
        filesArgument.Arity = ArgumentArity.OneOrMore;
        
        var outputOption = new Option<string>("--output", "Output path for the archive file");
        outputOption.AddAlias("-o");
        outputOption.IsRequired = true;
        
        var pollIntervalOption = new Option<int>("--poll-interval", () => 2000, "Polling interval in milliseconds");
        pollIntervalOption.AddAlias("-i");
        
        var timeoutOption = new Option<int>("--timeout", () => 300000, "Timeout in milliseconds (5 minutes default)");
        timeoutOption.AddAlias("-t");
        
        var command = new Command("auto-archive", "Automatically create archive, wait for completion, and download")
        {
            filesArgument,
            outputOption,
            pollIntervalOption,
            timeoutOption
        };
        
        command.SetHandler(async (string[] files, string output, int pollInterval, int timeout) =>
        {
            try
            {
                var apiClient = serviceProvider.GetRequiredService<ApiClient>();
                
                // Step 1: Create archive
                Console.WriteLine($"Creating archive for {files.Length} files...");
                var archiveId = await apiClient.CreateArchiveAsync(files.ToList());
                Console.WriteLine($"Archive task created with ID: {archiveId}");
                
                // Step 2: Wait for completion
                Console.WriteLine("Waiting for archive creation to complete...");
                var startTime = DateTime.UtcNow;
                
                while (true)
                {
                    var status = await apiClient.GetArchiveStatusAsync(archiveId);
                    
                    switch (status.Status)
                    {
                        case ArchiveStatus.Pending:
                            Console.WriteLine("Archive is pending...");
                            break;
                        case ArchiveStatus.Processing:
                            Console.WriteLine($"Processing... {status.Progress}%");
                            break;
                        case ArchiveStatus.Ready:
                            Console.WriteLine("Archive is ready!");
                            goto Download;
                        case ArchiveStatus.Failed:
                            Console.WriteLine($"Archive creation failed: {status.Message ?? "Unknown error"}");
                            Environment.Exit(1);
                            return;
                    }
                    
                    // Check timeout
                    if (DateTime.UtcNow - startTime > TimeSpan.FromMilliseconds(timeout))
                    {
                        Console.WriteLine($"Timeout reached ({timeout}ms). Archive creation is taking too long.");
                        Environment.Exit(1);
                        return;
                    }
                    
                    await Task.Delay(pollInterval);
                }
                
                Download:
                // Step 3: Download archive
                Console.WriteLine($"Downloading archive to: {output}");
                
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(output);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                using var stream = await apiClient.DownloadArchiveAsync(archiveId);
                using var fileStream = new FileStream(output, FileMode.Create, FileAccess.Write);
                
                await stream.CopyToAsync(fileStream);
                
                Console.WriteLine($"Archive downloaded successfully to: {output}");
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
        }, filesArgument, outputOption, pollIntervalOption, timeoutOption);
        
        return command;
    }
}
