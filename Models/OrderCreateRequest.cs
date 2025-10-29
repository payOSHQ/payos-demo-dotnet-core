using System.ComponentModel.DataAnnotations;

using PayOS.Models.V2.PaymentRequests;

namespace PayOSDemo.Models;

public class OrderCreateRequest
{
    [Required]
    public long TotalAmount { get; set; }

    public string? Description { get; set; }

    public string? ReturnUrl { get; set; }

    public string? CancelUrl { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerCompanyName { get; set; }

    public string? BuyerEmail { get; set; }

    public string? BuyerPhone { get; set; }

    public string? BuyerAddress { get; set; }

    public DateTimeOffset? ExpiredAt { get; set; }

    public bool? BuyerNotGetInvoice { get; set; }

    public TaxPercentage? TaxPercentage { get; set; }

    [Required]
    public List<OrderItemCreateRequest> Items { get; set; } = [];
}

public class OrderItemCreateRequest
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