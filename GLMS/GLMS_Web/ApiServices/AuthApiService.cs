using System.Net.Http.Json;

namespace GLMS_Web.ApiServices
{
    //Code Attribution for sending HTTP requests 
    //Link: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0

    // Handles authentication requests between MVC and API
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new Exception("API Base URL missing.");
        }

        // Sends login request to the API
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}Auth/login",
                    request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<LoginResponse>();
        }
    }

    // Login request DTO
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    // Login response DTO
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}