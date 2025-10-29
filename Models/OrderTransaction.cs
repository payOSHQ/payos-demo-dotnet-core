using System.ComponentModel.DataAnnotations;

namespace PayOSDemo.Models;

public class OrderTransaction
{
    public int Id { get; set; }

    // Reference fields
    public int OrderId { get; set; }
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = "";

    // Transaction details
    public string Reference { get; set; } = "";
    public long Amount { get; set; }
    public string AccountNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTimeOffset TransactionDateTime { get; set; }
    public string? VirtualAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? CounterAccountBankId { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
}
