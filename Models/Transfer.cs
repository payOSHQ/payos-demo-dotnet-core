using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using PayOS.Models.V1.Payouts;

namespace PayOSDemo.Models;

public class Transfer
{
    [Required]
    public string Id { get; set; } = "";

    public string? PayoutId { get; set; } = "";

    public int? TotalCredit { get; set; }

    public List<string>? Category { get; set; }

    public PayoutApprovalState ApprovalState { get; set; } = PayoutApprovalState.Drafting;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public List<TransferTransaction> Transactions { get; set; } = [];
}
