using System.ComponentModel.DataAnnotations;

using PayOS.Models.V2.PaymentRequests;

namespace PayOSDemo.Models;

public class Order
{
    public int Id { get; set; }

    public long OrderCode { get; set; }

    [Required]
    public long TotalAmount { get; set; }

    // Basic order information
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
    public string? Description { get; set; }

    // Payment link related properties
    public string? PaymentLinkId { get; set; }
    public string? QrCode { get; set; }
    public string? CheckoutUrl { get; set; }
    public PaymentLinkStatus Status { get; set; } = PaymentLinkStatus.Pending;

    // Amount tracking
    public long Amount { get; set; }
    public long AmountPaid { get; set; } = 0;
    public long AmountRemaining { get; set; } = 0;

    // Buyer information
    public string? BuyerName { get; set; }
    public string? BuyerCompanyName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerAddress { get; set; }

    // Payment link details
    public string? Bin { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? Currency { get; set; } = "VND";

    // URLs
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }

    // Timestamps
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
    public DateTimeOffset? LastTransactionUpdate { get; set; }

    // Cancellation
    public string? CancellationReason { get; set; }

    // Invoice settings
    public bool? BuyerNotGetInvoice { get; set; }
    public TaxPercentage? TaxPercentage { get; set; }

    [Required]
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    [Required]
    public string? Name { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required]
    public long Price { get; set; }

    public string? Unit { get; set; }
    public TaxPercentage? TaxPercentage { get; set; }
}
