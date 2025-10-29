using PayOSDemo.Models;
using PayOS.Models.V2.PaymentRequests;

namespace PayOSDemo.Services;

public static class OrderService
{
    private static readonly List<Order> Orders = [];

    public static List<Order> GetAllOrders()
    {
        return Orders;
    }

    public static Order? GetOrderById(int id)
    {
        return Orders.FirstOrDefault(o => o.Id == id);
    }

    public static Order? GetOrderByOrderCode(long orderCode)
    {
        return Orders.FirstOrDefault(o => o.OrderCode == orderCode);
    }

    public static Order? GetOrderByPaymentLinkId(string paymentLinkId)
    {
        return Orders.FirstOrDefault(o => o.PaymentLinkId == paymentLinkId);
    }

    public static void CreateOrder(Order order)
    {
        order.Id = Orders.Count + 1;
        Orders.Add(order);
    }

    public static bool UpdateOrder(int id, Order updatedOrder)
    {
        var order = Orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return false;

        // Basic order information
        order.OrderCode = updatedOrder.OrderCode;
        order.TotalAmount = updatedOrder.TotalAmount;
        order.OrderDate = updatedOrder.OrderDate;
        order.Description = updatedOrder.Description;
        order.Items = updatedOrder.Items;

        // Payment link related properties
        order.PaymentLinkId = updatedOrder.PaymentLinkId;
        order.QrCode = updatedOrder.QrCode;
        order.CheckoutUrl = updatedOrder.CheckoutUrl;
        order.Status = updatedOrder.Status;

        // Amount tracking
        order.Amount = updatedOrder.Amount;
        order.AmountPaid = updatedOrder.AmountPaid;
        order.AmountRemaining = updatedOrder.AmountRemaining;

        // Buyer information
        order.BuyerName = updatedOrder.BuyerName;
        order.BuyerCompanyName = updatedOrder.BuyerCompanyName;
        order.BuyerEmail = updatedOrder.BuyerEmail;
        order.BuyerPhone = updatedOrder.BuyerPhone;
        order.BuyerAddress = updatedOrder.BuyerAddress;

        // Payment link details
        order.Bin = updatedOrder.Bin;
        order.AccountNumber = updatedOrder.AccountNumber;
        order.AccountName = updatedOrder.AccountName;
        order.Currency = updatedOrder.Currency;

        // URLs
        order.ReturnUrl = updatedOrder.ReturnUrl;
        order.CancelUrl = updatedOrder.CancelUrl;

        // Timestamps
        order.CreatedAt = updatedOrder.CreatedAt;
        order.CanceledAt = updatedOrder.CanceledAt;
        order.ExpiredAt = updatedOrder.ExpiredAt;
        order.LastTransactionUpdate = updatedOrder.LastTransactionUpdate;

        // Cancellation
        order.CancellationReason = updatedOrder.CancellationReason;

        // Invoice settings
        order.BuyerNotGetInvoice = updatedOrder.BuyerNotGetInvoice;
        order.TaxPercentage = updatedOrder.TaxPercentage;

        return true;
    }

    public static bool DeleteOrder(int id)
    {
        var order = Orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return false;
        Orders.Remove(order);
        return true;
    }
}