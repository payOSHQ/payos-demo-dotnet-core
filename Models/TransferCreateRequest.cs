using System.ComponentModel.DataAnnotations;

namespace PayOSDemo.Models;

public class TransferCreateRequest
{
    [Required]
    public long Amount { get; set; }

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public string ToBin { get; set; } = "";

    [Required]
    public string ToAccountNumber { get; set; } = "";

    public string? ToAccountName { get; set; }

    public List<string>? Category { get; set; }
}