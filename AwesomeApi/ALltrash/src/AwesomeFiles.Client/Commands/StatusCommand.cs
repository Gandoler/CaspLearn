using System.CommandLine;
using AwesomeFiles.Common.DTOs;
using System.Text.Json;

namespace AwesomeFiles.Client.Commands;

public class StatusCommand : BaseCommand
{
    public static Command Create()
    {
        var command = new Command("status", "Check the status of an archive task");
        
        var idArgument = new Argument<string>("id", "Archive task ID");
        command.AddArgument(idArgument);
        
        command.SetHandler(async (string id, string server) =>
        {
            try
            {
                if (!Guid.TryParse(id, out var taskId))
                {
                    WriteError("Invalid task ID format. Must be a valid GUID.");
                    return;
                }

                using var httpClient = CreateHttpClient(server);
                var response = await httpClient.GetAsync($"/api/archives/{taskId}/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var status = JsonSerializer.Deserialize<ArchiveStatusResponse>(json, JsonOptions);
                    
                    if (status != null)
                    {
                        Console.WriteLine($"Task ID: {status.Id}");
                        Console.WriteLine($"Status: {GetStatusDisplay(status.Status)}");
                        Console.WriteLine($"Progress: {status.Progress}%");
                        
                        if (!string.IsNullOrEmpty(status.Message))
                        {
                            Console.WriteLine($"Message: {status.Message}");
                        }
                        
                        if (status.Status == AwesomeFiles.Common.Models.ArchiveStatus.Ready)
                        {
                            WriteSuccess("Archive is ready for download!");
                        }
                        else if (status.Status == AwesomeFiles.Common.Models.ArchiveStatus.Failed)
                        {
                            WriteError("Archive creation failed.");
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    WriteError("Archive task not found.");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    WriteError($"Failed to get status: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error checking status: {ex.Message}");
            }
        }, idArgument, new Option<string>("--server", () => "https://localhost:7000", "API server URL"));
        
        return command;
    }

    private static string GetStatusDisplay(AwesomeFiles.Common.Models.ArchiveStatus status)
    {
        return status switch
        {
            AwesomeFiles.Common.Models.ArchiveStatus.Pending => "⏳ Pending",
            AwesomeFiles.Common.Models.ArchiveStatus.Processing => "🔄 Processing",
            AwesomeFiles.Common.Models.ArchiveStatus.Ready => "✅ Ready",
            AwesomeFiles.Common.Models.ArchiveStatus.Failed => "❌ Failed",
            _ => status.ToString()
        };
    }
}
