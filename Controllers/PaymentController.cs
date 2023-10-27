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
public class PaymentController : ControllerBase
{
    private readonly OrderModel _orderModel;
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration)
    {
        _configuration = configuration;

        _orderModel = new OrderModel(configuration);
    }

    [HttpPost("payos_transfer_handler")]
    public async Task<IActionResult> payosTransferHandler(UpdateTranserRequest body)
    {
        try
        {
            
            Console.WriteLine("Payements transfer");
            UpdateTranserData data = body.data;
            string signature = body.signature;
            if (data == null)
            {
                throw new Exception("Không có dữ liệu");
            }
            if (data.description == "Ma giao dich thu nghiem")
            {
                return Ok(new Response(0, "Ok", null));
            }

            if (signature == null)
            {
                throw new Exception("Không có chữ ký");
            }


            String checksumKey = _configuration["Environment:PAYOS_CHECKSUM_KEY"]?? throw new Exception("Cannot find environment");

            string signData = Utils.CreateSignatureFromObj(JObject.Parse(JsonConvert.SerializeObject(data)), checksumKey);

            if (signData != signature)
            {
                throw new Exception("Chữ ký không hợp lệ");
            }
            
            _orderModel.updateOrderWebhook(data.orderCode, data.reference, data.code, JsonConvert.SerializeObject(body));

            return Ok(new Response(0, "Ok", null));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Ok(new Response(-1, "fail", null));
        }

    }
}