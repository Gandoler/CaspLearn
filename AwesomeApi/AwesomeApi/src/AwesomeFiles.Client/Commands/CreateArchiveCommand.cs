using System.CommandLine;
using AwesomeFiles.Common.DTOs;
using System.Text.Json;

namespace AwesomeFiles.Client.Commands;

public class CreateArchiveCommand : BaseCommand
{
    public static Command Create()
    {
        var command = new Command("create-archive", "Create a new archive from specified files");
        
        var filesArgument = new Argument<string[]>("files", "Files to include in the archive");
        command.AddArgument(filesArgument);
        
        command.SetHandler(async (string[] files, string server) =>
        {
            try
            {
                using var httpClient = CreateHttpClient(server);
                
                var request = new CreateArchiveRequest { Files = files.ToList() };
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync("/api/archives", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CreateArchiveResponse>(responseJson, JsonOptions);
                    
                    if (result != null)
                    {
                        WriteSuccess($"Archive task created successfully!");
                        Console.WriteLine($"Task ID: {result.Id}");
                        Console.WriteLine($"Files: {string.Join(", ", files)}");
                        Console.WriteLine();
                        WriteInfo("Use 'status <task-id>' to check progress or 'download <task-id> <path>' to download when ready.");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    WriteError($"Failed to create archive: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error creating archive: {ex.Message}");
            }
        }, filesArgument, new Option<string>("--server", () => "https://localhost:7000", "API server URL"));
        
        return command;
    }
}
