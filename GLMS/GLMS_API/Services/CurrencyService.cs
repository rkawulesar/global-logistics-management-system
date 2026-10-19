using System.Text.Json;

namespace GLMS_API.Services
{
    //Code Attribution for external API integration using HttpClient and JSON parsing
    //Links:
    //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
    //https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument
    //https://www.exchangerate-api.com/docs/standard-requests
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<decimal?> GetUsdToZarRateAsync()
        {
            try
            {
                // Gets the API key from configuration / user secrets
                var apiKey = _configuration["ExchangeRateApi:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    return null;
                }

                // Calls the ExchangeRate-API using USD as base currency
                var url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/USD";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Checks the API result
                var result = doc.RootElement.GetProperty("result").GetString();
                if (result != "success")
                {
                    return null;
                }

                // Reads ZAR conversion rate
                var zarRate = doc.RootElement
                    .GetProperty("conversion_rates")
                    .GetProperty("ZAR")
                    .GetDecimal();

                return zarRate;
            }
            catch
            {
                return null;
            }
        }
    }
}