using System.ComponentModel.DataAnnotations;

namespace AwesomeFiles.Common.DTOs;

public class CreateArchiveRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one file must be specified")]
    public List<string> Files { get; set; } = new();
}
