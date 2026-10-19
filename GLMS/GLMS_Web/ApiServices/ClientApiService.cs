using System.Net.Http.Json;
using GLMS_Web.Models;

namespace GLMS_Web.ApiServices
{
    //Code Attribution for sending HTTP requests and JSON serialization
    //Link: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0
    public class ClientApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ClientApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new Exception("API Base URL missing.");
        }

        // Retrieves all clients from the API
        public async Task<List<Client>> GetClientsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Clients");
            if (!response.IsSuccessStatusCode)
                return new List<Client>();
            return await response.Content.ReadFromJsonAsync<List<Client>>()
                ?? new List<Client>();
        }

        // Retrieves a single client by ID
        public async Task<Client?> GetClientByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Clients/{id}");
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<Client>();
        }

        // Sends a request to create a new client
        public async Task<CreateClientResponse?> CreateClientAsync(Client client)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}Clients",
                client);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CreateClientResponse>();
        }

        // Sends a request to update an existing client
        public async Task<bool> UpdateClientAsync(Client client)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"{_baseUrl}Clients/{client.Id}",
                client);
            return response.IsSuccessStatusCode;
        }

        // Sends a request to delete a client
        public async Task<bool> DeleteClientAsync(int id)
        {
            var response = await _httpClient.DeleteAsync(
                $"{_baseUrl}Clients/{id}");
            return response.IsSuccessStatusCode;
        }

        public class CreateClientResponse
        {
            public Client Client { get; set; } = new();
            public string Username { get; set; } = string.Empty;
            public string TemporaryPassword { get; set; } = string.Empty;
        }
    }
}