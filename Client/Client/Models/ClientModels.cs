namespace AwesomeFiles.Client.Models;

public class FileListResponse
{
    public List<FileMetadata> Files { get; set; } = new();
}

public class CreateArchiveRequest
{
    public List<string> Files { get; set; } = new();
}

public class CreateArchiveResponse
{
    public Guid Id { get; set; }
}

public class ArchiveStatusResponse
{
    public Guid Id { get; set; }
    public ArchiveStatus Status { get; set; }
    public int Progress { get; set; }
    public string? Message { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public List<string>? Files { get; set; }
    public string? Status { get; set; }
}
