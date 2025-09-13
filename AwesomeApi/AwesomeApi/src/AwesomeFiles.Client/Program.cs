using System.CommandLine;
using AwesomeFiles.Client.Commands;

var rootCommand = new RootCommand("Awesome Files Client - CLI for managing file archives");

// Add commands
rootCommand.AddCommand(ListCommand.Create());
rootCommand.AddCommand(CreateArchiveCommand.Create());
rootCommand.AddCommand(StatusCommand.Create());
rootCommand.AddCommand(DownloadCommand.Create());
rootCommand.AddCommand(AutoCreateAndDownloadCommand.Create());

// Set global options
var serverOption = new Option<string>("--server", () => "https://localhost:7000", "API server URL");
rootCommand.AddGlobalOption(serverOption);

return await rootCommand.InvokeAsync(args);
