using PayOSDemo.Models;

namespace PayOSDemo.Services;

public static class OrderInvoiceService
{
    private static readonly List<OrderInvoice> Invoices = [];
    private static int _nextId = 1;

    public static List<OrderInvoice> GetAllInvoices()
    {
        return Invoices;
    }

    public static OrderInvoice? GetInvoiceById(int id)
    {
        return Invoices.FirstOrDefault(i => i.Id == id);
    }

    public static OrderInvoice? GetInvoiceByInvoiceId(string invoiceId)
    {
        return Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);
    }

    public static List<OrderInvoice> GetInvoicesByOrderId(int orderId)
    {
        return Invoices.Where(i => i.OrderId == orderId).ToList();
    }

    public static List<OrderInvoice> GetInvoicesByOrderCode(long orderCode)
    {
        return Invoices.Where(i => i.OrderCode == orderCode).ToList();
    }

    public static List<OrderInvoice> GetInvoicesByPaymentLinkId(string paymentLinkId)
    {
        return Invoices.Where(i => i.PaymentLinkId == paymentLinkId).ToList();
    }

    public static void CreateInvoice(OrderInvoice invoice)
    {
        invoice.Id = _nextId++;
        Invoices.Add(invoice);
    }

    public static void CreateInvoices(List<OrderInvoice> invoices)
    {
        foreach (var invoice in invoices)
        {
            CreateInvoice(invoice);
        }
    }

    public static bool UpdateInvoice(int id, OrderInvoice updatedInvoice)
    {
        var invoice = Invoices.FirstOrDefault(i => i.Id == id);
        if (invoice == null) return false;

        invoice.OrderId = updatedInvoice.OrderId;
        invoice.OrderCode = updatedInvoice.OrderCode;
        invoice.PaymentLinkId = updatedInvoice.PaymentLinkId;
        invoice.InvoiceId = updatedInvoice.InvoiceId;
        invoice.InvoiceNumber = updatedInvoice.InvoiceNumber;
        invoice.IssuedTimestamp = updatedInvoice.IssuedTimestamp;
        invoice.IssuedDatetime = updatedInvoice.IssuedDatetime;
        invoice.TransactionId = updatedInvoice.TransactionId;
        invoice.ReservationCode = updatedInvoice.ReservationCode;
        invoice.CodeOfTax = updatedInvoice.CodeOfTax;

        return true;
    }

    public static bool DeleteInvoice(int id)
    {
        var invoice = Invoices.FirstOrDefault(i => i.Id == id);
        if (invoice == null) return false;
        Invoices.Remove(invoice);
        return true;
    }

    public static bool DeleteInvoicesByOrderId(int orderId)
    {
        var invoicesToRemove = Invoices.Where(i => i.OrderId == orderId).ToList();
        foreach (var invoice in invoicesToRemove)
        {
            Invoices.Remove(invoice);
        }
        return invoicesToRemove.Count > 0;
    }
}