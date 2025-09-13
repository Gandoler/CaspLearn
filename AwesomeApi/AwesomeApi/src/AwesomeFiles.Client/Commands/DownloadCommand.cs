using System.CommandLine;

namespace AwesomeFiles.Client.Commands;

public class DownloadCommand : BaseCommand
{
    public static Command Create()
    {
        var command = new Command("download", "Download a completed archive");
        
        var idArgument = new Argument<string>("id", "Archive task ID");
        var pathArgument = new Argument<string>("path", "Local path to save the archive");
        
        command.AddArgument(idArgument);
        command.AddArgument(pathArgument);
        
        command.SetHandler(async (string id, string path, string server) =>
        {
            try
            {
                if (!Guid.TryParse(id, out var taskId))
                {
                    WriteError("Invalid task ID format. Must be a valid GUID.");
                    return;
                }

                using var httpClient = CreateHttpClient(server);
                var response = await httpClient.GetAsync($"/api/archives/{taskId}/download");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(path, content);
                    
                    WriteSuccess($"Archive downloaded successfully to: {path}");
                    Console.WriteLine($"Size: {FormatBytes(content.Length)}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    WriteError("Archive not found or task ID is invalid.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    WriteError("Archive is not ready for download yet. Check status first.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    WriteError($"Failed to download archive: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error downloading archive: {ex.Message}");
            }
        }, idArgument, pathArgument, new Option<string>("--server", () => "https://localhost:7000", "API server URL"));
        
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
