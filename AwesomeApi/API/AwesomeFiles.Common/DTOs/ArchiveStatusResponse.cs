using AwesomeFiles.Common.Models;

namespace AwesomeFiles.Common.DTOs;

public class ArchiveStatusResponse
{
    public Guid Id { get; set; }
    public ArchiveStatus Status { get; set; }
    public int Progress { get; set; }
    public string? Message { get; set; }
}
