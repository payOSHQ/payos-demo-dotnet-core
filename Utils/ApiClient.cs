using System.Text;
namespace PayOSNetCore.Utils;
public class MyApiClient
{
    private readonly HttpClient _httpClient;

    public MyApiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> CallApiWithJsonBodyAndHeadersAsync(string apiUrl, string jsonBody, Dictionary<string, string> headers)
    {
        try
        {
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Content = httpContent;

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // response.EnsureSuccessStatusCode(); // Kiểm tra xem phản hồi có thành công không (status code 2xx)

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
            throw;
        }
    }
}