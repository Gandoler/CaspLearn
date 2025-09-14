namespace AwesomeFiles.Client.Models;

public class FileMetadata
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime Modified { get; set; }
}
