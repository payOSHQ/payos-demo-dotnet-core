namespace NetCoreDemo.Controllers;

using Microsoft.AspNetCore.Mvc;
using NetCoreDemo.Types;
using Net.payOS;
using Net.payOS.Types;
[Route("[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly PayOS _payOS;
    public OrderController(PayOS payOS)
    {
        _payOS = payOS;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
    {
        try
        {
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            ItemData item = new ItemData(body.productName, 1, body.price);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);
            PaymentData paymentData = new PaymentData(orderCode, body.price, body.description, items, body.cancelUrl, body.returnUrl);

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

            return Ok(new Response(0, "success", createPayment));
        }
        catch (System.Exception exception)
        {
            Console.WriteLine(exception);
            return Ok(new Response(-1, "fail", null));
        }
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder([FromRoute] int orderId)
    {
        try
        {
            PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInfomation(orderId);
            return Ok(new Response(0, "Ok", paymentLinkInformation));
        }
        catch (System.Exception exception)
        {

            Console.WriteLine(exception);
            return Ok(new Response(-1, "fail", null));
        }

    }
    [HttpPut("{orderId}")]
    public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
    {
        try
        {
            PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId);
            return Ok(new Response(0, "Ok", paymentLinkInformation));
        }
        catch (System.Exception exception)
        {

            Console.WriteLine(exception);
            return Ok(new Response(-1, "fail", null));
        }

    }
    [HttpPost("confirm-webhook")]
    public async Task<IActionResult> ConfirmWebhook(ConfirmWebhook body)
    {
        try
        {
            await _payOS.confirmWebhook(body.webhook_url);
            return Ok(new Response(0, "Ok", null));
        }
        catch (System.Exception exception)
        {

            Console.WriteLine(exception);
            return Ok(new Response(-1, "fail", null));
        }

    }
}
