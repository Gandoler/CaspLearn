using System.CommandLine;
using AwesomeFiles.Common.Models;
using System.Text.Json;

namespace AwesomeFiles.Client.Commands;

public class ListCommand : BaseCommand
{
    public static Command Create()
    {
        var command = new Command("list", "List all available files");
        
        command.SetHandler(async (string server) =>
        {
            try
            {
                using var httpClient = CreateHttpClient(server);
                var response = await httpClient.GetAsync("/api/files");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var files = JsonSerializer.Deserialize<List<FileMetadata>>(json, JsonOptions);
                    
                    if (files != null && files.Any())
                    {
                        WriteInfo($"Found {files.Count} files:");
                        Console.WriteLine();
                        
                        foreach (var file in files.OrderBy(f => f.Name))
                        {
                            Console.WriteLine($"  {file.Name}");
                            Console.WriteLine($"    Size: {FormatBytes(file.Size)}");
                            Console.WriteLine($"    Modified: {file.Modified:yyyy-MM-dd HH:mm:ss}");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        WriteInfo("No files found.");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    WriteError($"Failed to list files: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error listing files: {ex.Message}");
            }
        }, new Option<string>("--server", () => "https://localhost:7000", "API server URL"));
        
        return command;
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
