namespace PayOSNetCore.Controllers;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayOSNetCore.Models;
using PayOSNetCore.Types;
using PayOSNetCore.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

[Route("[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly OrderModel _orderModel;
    private readonly IConfiguration _configuration;

    public OrderController(IConfiguration configuration)
    {
        _configuration = configuration;

        _orderModel = new OrderModel(configuration);
    }


    [HttpGet("{orderId}")]
    public IActionResult GetOrder([FromRoute] int orderId)
    {
        
        List<Dictionary<String, dynamic>> orders = _orderModel.getOrder(orderId);
        // Console.WriteLine(orders[0]["webhook_snapshot"]);
        if (orders.Count == 0) return Ok(new Response(1, "Fail", null));
        WebhookSnapshot webhookSnapshot = null;
        if (orders[0]["webhook_snapshot"] != null){

            webhookSnapshot =JsonConvert.DeserializeObject<WebhookSnapshot>(orders[0]["webhook_snapshot"]);
        }
        List<Item> items = JsonConvert.DeserializeObject<List<Item>>(orders[0]["items"]);
        return Ok(new Response(0, "Ok", new OrderInfo(
            orders[0]["id"],
            orders[0]["status"],
            items,
            orders[0]["amount"],
            orders[0]["ref_id"],
            orders[0]["description"],
            orders[0]["transaction_when"],
            orders[0]["payment_link_id"],
            orders[0]["transaction_code"],
            orders[0]["created_at"],
            orders[0]["updated_at"],
            webhookSnapshot
            )));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
    {
        try
        {
            
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            Item item = new Item(body.productName, 1, body.price);
            List<Item> items = new List<Item>();
            items.Add(item);
            String checksumKey = _configuration["Environment:PAYOS_CHECKSUM_KEY"];

            BodyRequest bodyRequest = new BodyRequest(orderCode, body.price, body.description, items, body.cancelUrl, body.returnUrl, "");
            string signature = Utils.CreateSignatureOfPaymentRequest(bodyRequest, checksumKey);
            string bodyRequestString = JsonConvert.SerializeObject(bodyRequest);
            JObject bodyRequestJson = JObject.Parse(bodyRequestString);
            bodyRequestJson["signature"] = signature;

            //Body request
            bodyRequestString = bodyRequestJson.ToString();

            //Headers request
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("x-client-id", _configuration["Environment:PAYOS_CLIENT_ID"]);
            headers.Add("x-api-key", _configuration["Environment:PAYOS_API_KEY"]);

            //Make Request

            MyApiClient client = new MyApiClient();
            String responseBody = await client.CallApiWithJsonBodyAndHeadersAsync(
                _configuration["Environment:PAYOS_CREATE_PAYMENT_LINK_URL"],
                bodyRequestString,
                headers);
            JObject responseBodyJson = JObject.Parse(responseBody);

            if ((string)responseBodyJson["code"] != "00")
            {
                throw new Exception("Invalid response");
            }

            String checkoutUrl = (string)responseBodyJson["data"]["checkoutUrl"];
            string paymentLinkResSignature = Utils.CreateSignatureFromObj((JObject)responseBodyJson["data"], checksumKey);

            if (paymentLinkResSignature != (string)responseBodyJson["signature"])
            {
                throw new Exception("Signature is not compatible");
            }
            _orderModel.createOrder(
                orderCode,
                JsonConvert.SerializeObject(items),
                body.price,
                body.description,
                (string)responseBodyJson?["data"]["paymentLinkId"]);

            return Ok(new Response(0, "success", new CheckoutUrl(checkoutUrl)));
        }
        catch (System.Exception exception)
        {
            Console.WriteLine(exception);
            return Ok(new Response(-1, "fail", null));
        }

    }
}