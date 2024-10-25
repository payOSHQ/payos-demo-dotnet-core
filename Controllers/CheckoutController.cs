namespace NetCoreDemo.Controllers;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
public class CheckoutController : Controller
{
    private readonly PayOS _payOS;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public CheckoutController(PayOS payOS, IHttpContextAccessor httpContextAccessor)
    {
        _payOS = payOS;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        // Trả về trang HTML có tên "MyView.cshtml"
        return View("index");
    }
    [HttpGet("/cancel")]
    public IActionResult Cancel()
    {
        // Trả về trang HTML có tên "MyView.cshtml"
        return View("cancel");
    }
    [HttpGet("/success")]
    public IActionResult Success()
    {
        // Trả về trang HTML có tên "MyView.cshtml"
        return View("success");
    }
    [HttpPost("/create-payment-link")]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            ItemData item = new ItemData("Mì tôm hảo hảo ly", 1, 1000);
            List<ItemData> items = new List<ItemData> { item };

            // Get the current request's base URL
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            PaymentData paymentData = new PaymentData(
                orderCode,
                2000,
                "Thanh toan don hang",
                items,
                $"{baseUrl}/cancel",
                $"{baseUrl}/success"
            );

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

            return Redirect(createPayment.checkoutUrl);
        }
        catch (System.Exception exception)
        {
            Console.WriteLine(exception);
            return Redirect("/");
        }
    }
}
