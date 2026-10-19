using System.Net.Http.Json;
using GLMS_Web.Models;

namespace GLMS_Web.ApiServices
{
    //Code Attribution for sending HTTP requests 
    //Link: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0
    public class ServiceRequestApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ServiceRequestApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new Exception("API Base URL missing.");
        }

        // Retrieves all service requests from the API (Admin)
        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}ServiceRequests");
            if (!response.IsSuccessStatusCode)
                return new List<ServiceRequest>();
            return await response.Content.ReadFromJsonAsync<List<ServiceRequest>>()
                ?? new List<ServiceRequest>();
        }

        // Retrieves a single service request by ID
        public async Task<ServiceRequest?> GetServiceRequestByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}ServiceRequests/{id}");
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<ServiceRequest>();
        }

        // Sends a request to create a new service request
        public async Task<bool> CreateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}ServiceRequests",
                serviceRequest);
            return response.IsSuccessStatusCode;
        }

        // Sends a request to update an existing service request
        public async Task<ServiceRequestUpdateResponse?> UpdateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"{_baseUrl}ServiceRequests/{serviceRequest.Id}",
                serviceRequest);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<ServiceRequestUpdateResponse>();
        }

        // Sends a request to delete a service request
        public async Task<bool> DeleteServiceRequestAsync(int id)
        {
            var response = await _httpClient.DeleteAsync(
                $"{_baseUrl}ServiceRequests/{id}");
            return response.IsSuccessStatusCode;
        }

        // Retrieves contracts for dropdown selection
        public async Task<List<ContractDropdownItem>> GetContractDropdownAsync()
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}ServiceRequests/contracts/dropdown");
            if (!response.IsSuccessStatusCode)
                return new List<ContractDropdownItem>();
            return await response.Content.ReadFromJsonAsync<List<ContractDropdownItem>>()
                ?? new List<ContractDropdownItem>();
        }

        // Retrieves the live USD to ZAR exchange rate
        public async Task<decimal?> GetUsdToZarRateAsync()
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}ServiceRequests/rate/usd-zar");
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadFromJsonAsync<decimal>();
        }

        // Retrieves service requests belonging to the logged-in client
        public async Task<List<ServiceRequest>> GetMyServiceRequestsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}ServiceRequests/my");
            if (!response.IsSuccessStatusCode)
                return new List<ServiceRequest>();
            return await response.Content.ReadFromJsonAsync<List<ServiceRequest>>()
                ?? new List<ServiceRequest>();
        }

        public class ServiceRequestUpdateResponse
        {
            public ServiceRequest? ServiceRequest { get; set; }
            public List<string> Notifications { get; set; } = new();
        }
    }
}