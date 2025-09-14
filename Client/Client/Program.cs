using System.CommandLine;
using AwesomeFiles.Client.Commands;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwesomeFiles.Client;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        var rootCommand = new RootCommand("Awesome Files Client - Console utility for managing file archives")
        {
            ListCommand.CreateCommand(host.Services),
            CreateArchiveCommand.CreateCommand(host.Services),
            StatusCommand.CreateCommand(host.Services),
            DownloadCommand.CreateCommand(host.Services),
            AutoArchiveCommand.CreateCommand(host.Services)
        };
        
        // Add global options
        var baseUrlOption = new Option<string>("--base-url", () => "http://localhost:5010", "Base URL of the API server");
        baseUrlOption.AddAlias("-u");
        rootCommand.AddGlobalOption(baseUrlOption);
        
        // Handle global options
        rootCommand.SetHandler((string baseUrl) =>
        {
            // Update configuration with the base URL
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            configuration["ApiSettings:BaseUrl"] = baseUrl;
        }, baseUrlOption);
        
        Console.WriteLine("Client was started. Press <Enter> to exit...");
        
        // If no arguments provided, show help
        if (args.Length == 0)
        {
            rootCommand.Invoke("--help");
            Console.WriteLine();
            Console.WriteLine("Press <Enter> to exit...");
            Console.ReadLine();
            return 0;
        }
        
        return await rootCommand.InvokeAsync(args);
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure HttpClient
                services.AddHttpClient<ApiClient>(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(10);
                });
                
                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Warning); // Only show warnings and errors by default
                });
                
                // Register services
                services.AddSingleton<ApiClient>();
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiSettings:BaseUrl"] = "http://localhost:5010"
                });
            });
}
