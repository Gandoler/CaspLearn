using System.CommandLine;
using AwesomeFiles.Common.DTOs;
using System.Text.Json;

namespace AwesomeFiles.Client.Commands;

public class AutoCreateAndDownloadCommand : BaseCommand
{
    public static Command Create()
    {
        var command = new Command("auto-create-and-download", "Create archive and automatically download when ready");
        
        var filesArgument = new Argument<string[]>("files", "Files to include in the archive");
        var pathArgument = new Argument<string>("path", "Local path to save the archive");
        
        command.AddArgument(filesArgument);
        command.AddArgument(pathArgument);
        
        command.SetHandler(async (string[] files, string path, string server) =>
        {
            try
            {
                using var httpClient = CreateHttpClient(server);
                
                // Step 1: Create archive
                WriteInfo("Creating archive...");
                var request = new CreateArchiveRequest { Files = files.ToList() };
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var createResponse = await httpClient.PostAsync("/api/archives", content);
                
                if (!createResponse.IsSuccessStatusCode)
                {
                    var error = await createResponse.Content.ReadAsStringAsync();
                    WriteError($"Failed to create archive: {createResponse.StatusCode} - {error}");
                    return;
                }

                var responseJson = await createResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CreateArchiveResponse>(responseJson, JsonOptions);
                
                if (result == null)
                {
                    WriteError("Failed to parse create archive response");
                    return;
                }

                WriteSuccess($"Archive task created: {result.Id}");
                
                // Step 2: Poll for completion
                WriteInfo("Waiting for archive to be ready...");
                var taskId = result.Id;
                var maxAttempts = 60; // 5 minutes max
                var delay = TimeSpan.FromSeconds(5);
                
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    await Task.Delay(delay);
                    
                    var statusResponse = await httpClient.GetAsync($"/api/archives/{taskId}/status");
                    
                    if (statusResponse.IsSuccessStatusCode)
                    {
                        var statusJson = await statusResponse.Content.ReadAsStringAsync();
                        var status = JsonSerializer.Deserialize<ArchiveStatusResponse>(statusJson, JsonOptions);
                        
                        if (status != null)
                        {
                            Console.Write($"\rStatus: {status.Status} ({status.Progress}%)");
                            
                            if (status.Status == AwesomeFiles.Common.Models.ArchiveStatus.Ready)
                            {
                                Console.WriteLine();
                                WriteSuccess("Archive is ready!");
                                break;
                            }
                            else if (status.Status == AwesomeFiles.Common.Models.ArchiveStatus.Failed)
                            {
                                Console.WriteLine();
                                WriteError($"Archive creation failed: {status.Message}");
                                return;
                            }
                        }
                    }
                    
                    if (attempt == maxAttempts - 1)
                    {
                        Console.WriteLine();
                        WriteError("Timeout waiting for archive to be ready");
                        return;
                    }
                }
                
                // Step 3: Download
                WriteInfo("Downloading archive...");
                var downloadResponse = await httpClient.GetAsync($"/api/archives/{taskId}/download");
                
                if (downloadResponse.IsSuccessStatusCode)
                {
                    var downloadContent = await downloadResponse.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(path, downloadContent);
                    
                    WriteSuccess($"Archive downloaded successfully to: {path}");
                    Console.WriteLine($"Size: {FormatBytes(downloadContent.Length)}");
                }
                else
                {
                    var error = await downloadResponse.Content.ReadAsStringAsync();
                    WriteError($"Failed to download archive: {downloadResponse.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error in auto-create-and-download: {ex.Message}");
            }
        }, filesArgument, pathArgument, new Option<string>("--server", () => "https://localhost:7000", "API server URL"));
        
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
