using System.CommandLine;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public class ListCommand
{
    public static Command CreateCommand(IServiceProvider serviceProvider)
    {
        var command = new Command("list", "List all available files");
        
        command.SetHandler(async () =>
        {
            try
            {
                var apiClient = serviceProvider.GetRequiredService<IApiClient>();
                var files = await apiClient.GetFilesAsync();
                
                if (files.Count == 0)
                {
                    Console.WriteLine("\nNo files available.");
                    return;
                }
                
                Console.WriteLine($"\nFound  {files.Count} files:");
                Console.WriteLine();
                
                foreach (var file in files)
                {
                    var size = FormatFileSize(file.Size);
                    var modified = file.Modified.ToString("yyyy-MM-dd HH:mm:ss");
                    Console.WriteLine($"\n   {file.Name} ({size}) - Modified: {modified}");
                }
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"\n Error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Unexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        });
        
        return command;
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
