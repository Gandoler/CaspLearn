using System;
using System.Collections.Generic;

namespace AwesomeFiles.Common.Models;

public enum ArchiveStatus
{
    Pending,
    Processing,
    Ready,
    Failed
}

public class ArchiveTask
{
    public Guid Id { get; set; }
    public ArchiveStatus Status { get; set; }
    public int Progress { get; set; }
    public string? Message { get; set; }
    public List<string> Files { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FilePath { get; set; }
}
