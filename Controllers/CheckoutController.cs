namespace PayOSNetCore.Controllers;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayOSNetCore.Models;
using PayOSNetCore.Types;
using PayOSNetCore.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

public class CheckoutController : Controller
{
    private readonly OrderModel _orderModel;
    private readonly IConfiguration _configuration;

    public CheckoutController(IConfiguration configuration)
    {
        _configuration = configuration;

        _orderModel = new OrderModel(configuration);
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
            Item item = new Item("Mì tôm hảo hảo ly", 1, 1000);
            List<Item> items = new List<Item>();
            items.Add(item);
            String checksumKey =
                _configuration["Environment:PAYOS_CHECKSUM_KEY"]
                ?? throw new Exception("Cannot find environment");

            BodyRequest bodyRequest = new BodyRequest(
                orderCode,
                1000,
                "Thanh toan don hang",
                items,
                "https://localhost:3002/cancel",
                "https://localhost:3002/success",
                ""
            );
            string signature = Utils.CreateSignatureOfPaymentRequest(bodyRequest, checksumKey);
            string bodyRequestString = JsonConvert.SerializeObject(bodyRequest);
            JObject bodyRequestJson = JObject.Parse(bodyRequestString);
            bodyRequestJson["signature"] = signature;

            //Body request
            bodyRequestString = bodyRequestJson.ToString();

            //Headers request
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(
                "x-client-id",
                _configuration["Environment:PAYOS_CLIENT_ID"]
                    ?? throw new Exception("Cannot find environment")
            );
            headers.Add(
                "x-api-key",
                _configuration["Environment:PAYOS_API_KEY"]
                    ?? throw new Exception("Cannot find environment")
            );

            //Make Request

            MyApiClient client = new MyApiClient();
            String responseBody = await client.CallApiWithJsonBodyAndHeadersAsync(
                _configuration["Environment:PAYOS_CREATE_PAYMENT_LINK_URL"]
                    ?? throw new Exception("Cannot find environment"),
                bodyRequestString,
                headers
            );
            JObject responseBodyJson = JObject.Parse(responseBody);

            if ((string?)responseBodyJson["code"] != "00")
            {
                throw new Exception("Invalid response");
            }

            String checkoutUrl = (string?)responseBodyJson?["data"]["checkoutUrl"];
            string paymentLinkResSignature = Utils.CreateSignatureFromObj(
                (JObject?)responseBodyJson["data"],
                checksumKey
            );

            if (paymentLinkResSignature != (string)responseBodyJson["signature"])
            {
                throw new Exception("Signature is not compatible");
            }
            return Redirect(checkoutUrl);
        }
        catch (System.Exception exception)
        {
            Console.WriteLine(exception);
            return Redirect("https://localhost:3002/");
        }
    }
}
