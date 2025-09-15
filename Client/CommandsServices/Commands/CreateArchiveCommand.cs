using System.CommandLine;
using AwesomeFiles.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeFiles.Client.Commands;

public class CreateArchiveCommand
{
    public static Command CreateCommand(IServiceProvider serviceProvider)
    {
        var filesArgument = new Argument<string[]>("files", "List of files to include in the archive");
        filesArgument.Arity = ArgumentArity.OneOrMore;
        
        var command = new Command("create-archive", "Create an archive from specified files")
        {
            filesArgument
        };
        
        command.SetHandler(async (string[] files) =>
        {
            try
            {
                var apiClient = serviceProvider.GetRequiredService<IApiClient>();
                var archiveId = await apiClient.CreateArchiveAsync(files.ToList());
                
                Console.WriteLine($"\nCreate archive task is started, id: {archiveId}");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error: {ex.Message}");
                Environment.Exit(1);
            }
        }, filesArgument);
        
        return command;
    }
}
