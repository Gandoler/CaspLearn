using System.CommandLine;
using System.Text.Json;

namespace AwesomeFiles.Client.Commands;

public abstract class BaseCommand
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    protected static HttpClient CreateHttpClient(string serverUrl)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(serverUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "AwesomeFiles.Client/1.0");
        return httpClient;
    }

    protected static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

    protected static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    protected static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
