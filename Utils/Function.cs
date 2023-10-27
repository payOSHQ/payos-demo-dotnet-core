using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using PayOSNetCore.Types;

namespace PayOSNetCore.Utils;

public class Utils
{
    private static string ConvertObjToQueryStr(JObject obj)
    {
        var queryString = new StringBuilder();

        foreach (var property in obj.Properties())
        {
            var key = property.Name;
            var value = property.Value;
            //default null if value is null
            string valueAsString = "null";

            //Case DateTime
            if (value.Type == JTokenType.Date)
            {
                DateTime dateValue = (DateTime)value;
                valueAsString = dateValue.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
            //Case String
            else if (value.Type == JTokenType.String)
            {
                valueAsString = value.Value<string>();
            }
            else if (value.Type != JTokenType.Null) // Remain type
            {
                valueAsString = value.ToString();
            }

            if (queryString.Length > 0)
            {
                queryString.Append('&');
            }

            queryString.Append(key).Append('=').Append(valueAsString);
        }

        return queryString.ToString();
    }

    private static JObject SortObjDataByKey(JObject obj)
    {
        if (obj.Type != JTokenType.Object)
        {
            return obj;
        }

        var orderedObject = new JObject(obj.Properties().OrderBy(x => x.Name));

        return orderedObject;
    }

    private static string GenerateHmacSHA256(string dataStr, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hmacBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataStr));

            var hexStringBuilder = new StringBuilder();
            foreach (var b in hmacBytes)
            {
                hexStringBuilder.Append(b.ToString("x2"));
            }

            return hexStringBuilder.ToString();
        }
    }

    public static string CreateSignatureFromObj(JObject data, string key)
    {
        // string json =
        //     @"
        //     {
        //         ""code"": ""00"",
        //         ""desc"": ""success"",
        //         ""data"": {
        //             ""id"": ""89e0cee536ff406189420bba8740ba0b"",
        //             ""orderCode"": 704057,
        //             ""amount"": 1000,
        //             ""amountPaid"": 0,
        //             ""amountRemaining"": 1000,
        //             ""status"": ""PENDING"",
        //             ""createdAt"": ""2023-10-27T14:45:04+07:00"",
        //             ""transactions"": [],
        //             ""canceledAt"": null,
        //             ""cancellationReason"": null
        //         },
        //         ""signature"": ""c6afc66375823b7341964f427b6199d90005ad3a375838d0fbbc095795a62b87""
        //     }";
        // JObject bodyRequestJson = JObject.Parse(json);
        // var sortedDataByKey1 = SortObjDataByKey((JObject)bodyRequestJson["data"]);
        // var dataQueryStr1 = ConvertObjToQueryStr(sortedDataByKey1);
        // string s = GenerateHmacSHA256(dataQueryStr1, key);
        // Console.WriteLine(dataQueryStr1);
        // Console.WriteLine(s);
        // Console.WriteLine(bodyRequestJson["signature"]);

        var sortedDataByKey = SortObjDataByKey(data);
        var dataQueryStr = ConvertObjToQueryStr(sortedDataByKey);
        return GenerateHmacSHA256(dataQueryStr, key);
    }

    public static string CreateSignatureOfPaymentRequest(BodyRequest data, string key)
    {
        var amount = data.amount;
        var cancelUrl = data.cancelUrl;
        var description = data.description;
        var orderCode = data.orderCode.ToString();
        var returnUrl = data.returnUrl;
        var dataStr =
            $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";

        return GenerateHmacSHA256(dataStr, key);
    }
}
