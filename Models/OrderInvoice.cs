using System.ComponentModel.DataAnnotations;

namespace PayOSDemo.Models;

public class OrderInvoice
{
    public int Id { get; set; }

    // Reference fields
    public int OrderId { get; set; }
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = "";

    // Invoice details
    public string InvoiceId { get; set; } = "";
    public string? InvoiceNumber { get; set; }
    public long? IssuedTimestamp { get; set; }
    public DateTime? IssuedDatetime { get; set; }
    public string? TransactionId { get; set; }
    public string? ReservationCode { get; set; }
    public string? CodeOfTax { get; set; }
}

public class OrderInvoicesInfo
{
    public List<OrderInvoice> Invoices { get; set; } = [];
}
