using Microsoft.AspNetCore.Mvc;
using PayOS;
using PayOS.Models.Webhooks;
using PayOS.Models.V2.PaymentRequests;
using PayOSDemo.Services;
using PayOSDemo.Models;

namespace PayOSDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController([FromKeyedServices("OrderClient")] PayOSClient client) : ControllerBase
{
    private readonly PayOSClient _client = client;

    private static DateTimeOffset ParseWebhookDateTime(string dateTimeString)
    {
        // Try to parse the standard ISO format first (2025-10-10T11:13:30+07:00)
        if (DateTimeOffset.TryParse(dateTimeString, out var result))
        {
            return result;
        }

        // Handle webhook format (2023-02-04 18:25:00)
        if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dateTime))
        {
            return new DateTimeOffset(dateTime, TimeSpan.FromHours(7));
        }

        // Fallback to current time if parsing fails
        return DateTimeOffset.Now;
    }

    [HttpPost("payment")]
    public async Task<ActionResult> VerifyPayment(Webhook webhook)
    {
        if (webhook == null)
        {
            return BadRequest("Webhook data is required");
        }

        try
        {
            var webhookData = await _client.Webhooks.VerifyAsync(webhook);
            if (webhookData.OrderCode == 123 && webhookData.Description == "VQRIO123" && webhookData.AccountNumber == "12345678")
            {
                return Ok(new { message = "Webhook processed successfully" });
            }

            var order = OrderService.GetOrderByOrderCode(webhookData.OrderCode);
            if (order != null)
            {
                var existingTransactions = OrderTransactionService.GetTransactionsByOrderId(order.Id);
                var transactionExists = existingTransactions.Any(t => t.Reference == webhookData.Reference);

                if (!transactionExists)
                {
                    var transaction = new OrderTransaction
                    {
                        OrderId = order.Id,
                        OrderCode = order.OrderCode,
                        PaymentLinkId = order.PaymentLinkId ?? "",
                        Reference = webhookData.Reference,
                        Amount = webhookData.Amount,
                        AccountNumber = webhookData.AccountNumber,
                        Description = webhookData.Description,
                        TransactionDateTime = ParseWebhookDateTime(webhookData.TransactionDateTime),
                        VirtualAccountName = webhookData.VirtualAccountName,
                        VirtualAccountNumber = webhookData.VirtualAccountNumber,
                        CounterAccountBankId = webhookData.CounterAccountBankId,
                        CounterAccountBankName = webhookData.CounterAccountBankName,
                        CounterAccountName = webhookData.CounterAccountName,
                        CounterAccountNumber = webhookData.CounterAccountNumber
                    };

                    OrderTransactionService.CreateTransaction(transaction);
                }

                var allTransactions = OrderTransactionService.GetTransactionsByOrderId(order.Id);
                var totalAmountPaid = allTransactions.Sum(t => t.Amount);

                order.AmountPaid = totalAmountPaid;
                order.AmountRemaining = order.Amount - totalAmountPaid;
                order.Status = order.AmountRemaining > 0 ? PaymentLinkStatus.Underpaid : PaymentLinkStatus.Paid;
                order.LastTransactionUpdate = DateTimeOffset.Now;

                OrderService.UpdateOrder(order.Id, order);
            }

            return Ok(new { message = "Webhook processed successfully", orderCode = webhookData.OrderCode });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook processing error: {ex.Message}");
            return Problem(ex.Message);
        }
    }
}