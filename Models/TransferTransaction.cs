using System;
using System.ComponentModel.DataAnnotations;

using PayOS.Models.V1.Payouts;

namespace PayOSDemo.Models;

public class TransferTransaction
{
    [Required]
    public string Id { get; set; } = "";

    public string? PayoutTransactionId { get; set; } = "";

    [Required]
    public long Amount { get; set; }

    [Required]
    public string Description { get; set; } = "";

    [Required]
    public string ToBin { get; set; } = "";

    [Required]
    public string ToAccountNumber { get; set; } = "";

    public string? ToAccountName { get; set; }

    public string? Reference { get; set; }

    public DateTimeOffset? TransactionDatetime { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorCode { get; set; }

    public PayoutTransactionState State { get; set; } = PayoutTransactionState.Received;
}
