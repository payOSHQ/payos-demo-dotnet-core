using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using PayOSDemo.Models;
using PayOSDemo.Services;

using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.V2.PaymentRequests.Invoices;

using PayOS;

namespace PayOSDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController([FromKeyedServices("OrderClient")] PayOSClient client) : ControllerBase
{
    private readonly PayOSClient _client = client;

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> Get(int id)
    {
        var order = OrderService.GetOrderById(id);
        if (order == null || string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return NotFound();
        }

        try
        {
            var paymentLink = await _client.PaymentRequests.GetAsync(order.PaymentLinkId);

            order.Status = paymentLink.Status;
            order.Amount = paymentLink.Amount;
            order.AmountPaid = paymentLink.AmountPaid;
            order.AmountRemaining = paymentLink.AmountRemaining;
            order.CreatedAt = DateTimeOffset.TryParse(paymentLink.CreatedAt, out var createdAt) ? createdAt : null;
            order.CanceledAt = DateTimeOffset.TryParse(paymentLink.CanceledAt, out var canceledAt) ? canceledAt : null;
            order.CancellationReason = paymentLink.CancellationReason;

            if (paymentLink.Transactions != null && paymentLink.Transactions.Count > 0)
            {
                OrderTransactionService.DeleteTransactionsByOrderId(order.Id);

                var transactions = paymentLink.Transactions.Select(t => new OrderTransaction
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    PaymentLinkId = order.PaymentLinkId,
                    Reference = t.Reference,
                    Amount = t.Amount,
                    AccountNumber = t.AccountNumber,
                    Description = t.Description,
                    TransactionDateTime = DateTimeOffset.TryParse(t.TransactionDateTime, out var transactionDateTime) ? transactionDateTime : DateTimeOffset.Now,
                    VirtualAccountName = t.VirtualAccountName,
                    VirtualAccountNumber = t.VirtualAccountNumber,
                    CounterAccountBankId = t.CounterAccountBankId,
                    CounterAccountBankName = t.CounterAccountBankName,
                    CounterAccountName = t.CounterAccountName,
                    CounterAccountNumber = t.CounterAccountNumber
                }).ToList();

                OrderTransactionService.CreateTransactions(transactions);
                order.LastTransactionUpdate = DateTimeOffset.Now;
            }

            OrderService.UpdateOrder(order.Id, order);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to retrieve order {id}", error = ex.Message });
        }

    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreatePayment(OrderCreateRequest request)
    {
        if (request == null)
        {
            return BadRequest("Order data is required");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
        var returnUrl = request.ReturnUrl ?? "https://your-domain.com/success";
        var cancelUrl = request.CancelUrl ?? "https://your-domain.com/cancel";
        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = request.TotalAmount,
            Description = request.Description ?? $"order {orderCode}",
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            BuyerName = request.BuyerName,
            BuyerCompanyName = request.BuyerCompanyName,
            BuyerEmail = request.BuyerEmail,
            BuyerPhone = request.BuyerPhone,
            BuyerAddress = request.BuyerAddress,
            ExpiredAt = request.ExpiredAt?.ToUnixTimeSeconds(),
            Items = [.. request.Items.Select(i => new PaymentLinkItem
            {
                Name = i.Name ?? "",
                Quantity = i.Quantity,
                Price = i.Price,
                Unit = i.Unit,
                TaxPercentage = i.TaxPercentage
            })]
        };

        if (request.BuyerNotGetInvoice.HasValue || request.TaxPercentage.HasValue)
        {
            paymentRequest.Invoice = new InvoiceRequest
            {
                BuyerNotGetInvoice = request.BuyerNotGetInvoice,
                TaxPercentage = request.TaxPercentage
            };
        }

        try
        {
            var paymentResponse = await _client.PaymentRequests.CreateAsync(paymentRequest);

            var order = new Order
            {
                Id = 0, // Will be set by service
                OrderCode = orderCode,
                TotalAmount = request.TotalAmount,
                Description = paymentResponse.Description,
                PaymentLinkId = paymentResponse.PaymentLinkId,
                QrCode = paymentResponse.QrCode,
                CheckoutUrl = paymentResponse.CheckoutUrl,
                Status = paymentResponse.Status,
                Amount = paymentResponse.Amount,
                AmountPaid = 0,
                AmountRemaining = paymentResponse.Amount,
                Bin = paymentResponse.Bin,
                AccountNumber = paymentResponse.AccountNumber,
                AccountName = paymentResponse.AccountName,
                Currency = paymentResponse.Currency,
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl,
                CreatedAt = DateTimeOffset.Now,
                BuyerName = request.BuyerName,
                BuyerCompanyName = request.BuyerCompanyName,
                BuyerEmail = request.BuyerEmail,
                BuyerPhone = request.BuyerPhone,
                BuyerAddress = request.BuyerAddress,
                ExpiredAt = request.ExpiredAt,
                BuyerNotGetInvoice = request.BuyerNotGetInvoice,
                TaxPercentage = request.TaxPercentage,
                Items = [.. request.Items.Select(i => new OrderItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Unit = i.Unit,
                    TaxPercentage = i.TaxPercentage
                })]
            };

            OrderService.CreateOrder(order);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create order", error = ex.Message });
        }
    }

    [HttpPost("{orderId}/cancel")]
    public async Task<ActionResult<PaymentLink>> CancelPayment(int orderId, string cancellationReason)
    {
        var order = OrderService.GetOrderById(orderId);
        if (order == null || string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return NotFound();
        }

        try
        {

            var paymentLink = await _client.PaymentRequests.CancelAsync(order.PaymentLinkId, cancellationReason ?? "Cancelled by user");

            order.Status = paymentLink.Status;
            order.Amount = paymentLink.Amount;
            order.AmountPaid = paymentLink.AmountPaid;
            order.AmountRemaining = paymentLink.AmountRemaining;
            order.CanceledAt = DateTimeOffset.TryParse(paymentLink.CanceledAt, out var canceledAt) ? canceledAt : null;
            order.CancellationReason = paymentLink.CancellationReason;

            if (paymentLink.Transactions != null && paymentLink.Transactions.Count > 0)
            {
                OrderTransactionService.DeleteTransactionsByOrderId(order.Id);

                var transactions = paymentLink.Transactions.Select(t => new OrderTransaction
                {
                    OrderId = order.Id,
                    OrderCode = order.OrderCode,
                    PaymentLinkId = order.PaymentLinkId,
                    Reference = t.Reference,
                    Amount = t.Amount,
                    AccountNumber = t.AccountNumber,
                    Description = t.Description,
                    TransactionDateTime = DateTimeOffset.TryParse(t.TransactionDateTime, out var transactionDateTime) ? transactionDateTime : DateTimeOffset.Now,
                    VirtualAccountName = t.VirtualAccountName,
                    VirtualAccountNumber = t.VirtualAccountNumber,
                    CounterAccountBankId = t.CounterAccountBankId,
                    CounterAccountBankName = t.CounterAccountBankName,
                    CounterAccountName = t.CounterAccountName,
                    CounterAccountNumber = t.CounterAccountNumber
                }).ToList();

                OrderTransactionService.CreateTransactions(transactions);
                order.LastTransactionUpdate = DateTimeOffset.Now;
            }

            OrderService.UpdateOrder(order.Id, order);
            return Ok(paymentLink);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to cancel order {orderId}", error = ex.Message });
        }
    }

    [HttpGet("{orderId}/invoices")]
    public async Task<ActionResult<OrderInvoicesInfo>> GetInvoices(int orderId)
    {
        var order = OrderService.GetOrderById(orderId);
        if (order == null || string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return NotFound();
        }
        try
        {
            var invoices = await _client.PaymentRequests.Invoices.GetAsync(order.PaymentLinkId);

            OrderInvoiceService.DeleteInvoicesByOrderId(order.Id);

            var orderInvoices = invoices.Invoices.Select(i => new OrderInvoice
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                PaymentLinkId = order.PaymentLinkId,
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                IssuedTimestamp = i.IssuedTimestamp,
                IssuedDatetime = i.IssuedDatetime,
                TransactionId = i.TransactionId,
                ReservationCode = i.ReservationCode,
                CodeOfTax = i.CodeOfTax
            }).ToList();

            OrderInvoiceService.CreateInvoices(orderInvoices);

            var orderInvoicesInfo = new OrderInvoicesInfo
            {
                Invoices = orderInvoices
            };

            return Ok(orderInvoicesInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to retrieve invoice for order {orderId}", error = ex.Message });
        }
    }

    [HttpGet("{orderId}/invoices/{invoiceId}/download")]
    public async Task<ActionResult> DownloadInvoice(int orderId, string invoiceId)
    {
        var order = OrderService.GetOrderById(orderId);
        if (order == null || string.IsNullOrEmpty(order.PaymentLinkId))
        {
            return NotFound();
        }

        var invoice = OrderInvoiceService.GetInvoicesByOrderId(orderId)
            .FirstOrDefault(i => i.InvoiceId == invoiceId);
        if (invoice == null)
        {
            return NotFound("Invoice not found for this order");
        }

        try
        {
            var invoiceFile = await _client.PaymentRequests.Invoices.DownloadAsync(invoiceId, order.PaymentLinkId);

            var fileName = invoiceFile.FileName ?? $"invoice_{invoiceId}.pdf";
            var contentType = invoiceFile.ContentType ?? "application/pdf";

            return File(invoiceFile.Content, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to download invoice {invoiceId}", error = ex.Message });
        }
    }
}