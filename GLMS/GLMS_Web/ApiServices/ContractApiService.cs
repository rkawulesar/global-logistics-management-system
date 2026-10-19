using System.Net.Http.Json;
using GLMS_Web.Models;

namespace GLMS_Web.ApiServices;
//Code Attribution for Uploading a file
//Link: https://www.c-sharpcorner.com/article/complete-file-upload-and-download-in-asp-net-core-mvc/
public class ContractApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ContractApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _baseUrl = configuration["ApiSettings:BaseUrl"]
            ?? throw new Exception("API Base URL missing.");
    }

    // GET all contracts (Admin)
    public async Task<List<Contract>> GetContractsAsync(
        string? status = null,
        string? serviceLevel = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        if (!string.IsNullOrEmpty(serviceLevel))
            queryParams.Add($"serviceLevel={serviceLevel}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var query = queryParams.Count > 0
            ? "Contracts?" + string.Join("&", queryParams)
            : "Contracts";

        var response = await _httpClient.GetAsync($"{_baseUrl}{query}");

        if (!response.IsSuccessStatusCode)
            return new List<Contract>();

        return await response.Content.ReadFromJsonAsync<List<Contract>>()
            ?? new List<Contract>();
    }

    // GET contract by ID
    public async Task<Contract?> GetContractByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}Contracts/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Contract>();
    }

    // POST contract
    public async Task<bool> CreateContractAsync(Contract contract, IFormFile? pdfFile)
    {
        using var form = BuildContractForm(contract, pdfFile);
        var response = await _httpClient.PostAsync($"{_baseUrl}Contracts", form);
        return response.IsSuccessStatusCode;
    }


    // PUT contract
    public async Task<bool> UpdateContractAsync(Contract contract, IFormFile? pdfFile)
    {
        using var form = BuildContractForm(contract, pdfFile);
        var response = await _httpClient.PutAsync($"{_baseUrl}Contracts/{contract.Id}", form);
        return response.IsSuccessStatusCode;
    }

    // PATCH status
    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var dto = new
        {
            Status = status
        };

        var response = await _httpClient.PatchAsJsonAsync(
            $"{_baseUrl}Contracts/{id}/status",
            dto);

        return response.IsSuccessStatusCode;
    }

    // DELETE contract
    public async Task<bool> DeleteContractAsync(int id)
    {
        var response = await _httpClient.DeleteAsync(
            $"{_baseUrl}Contracts/{id}");

        return response.IsSuccessStatusCode;
    }

    // GET my contracts (Client)
    public async Task<List<Contract>> GetMyContractsAsync(
        string? status = null,
        string? serviceLevel = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={status}");

        if (!string.IsNullOrEmpty(serviceLevel))
            queryParams.Add($"serviceLevel={serviceLevel}");

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var query = queryParams.Count > 0
            ? "Contracts/my?" + string.Join("&", queryParams)
            : "Contracts/my";

        var response = await _httpClient.GetAsync($"{_baseUrl}{query}");

        if (!response.IsSuccessStatusCode)
            return new List<Contract>();

        return await response.Content.ReadFromJsonAsync<List<Contract>>()
            ?? new List<Contract>();
    }


    // Builds the multipart form
    private MultipartFormDataContent BuildContractForm(Contract contract, IFormFile? pdfFile)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(contract.Id.ToString()), "Id");
        form.Add(new StringContent(contract.ClientId.ToString()), "ClientId");
        form.Add(new StringContent(contract.StartDate.ToString("o")), "StartDate");
        form.Add(new StringContent(contract.EndDate.ToString("o")), "EndDate");
        form.Add(new StringContent(contract.Status ?? ""), "Status");
        form.Add(new StringContent(contract.ServiceLevel ?? ""), "ServiceLevel");
        form.Add(new StringContent(contract.FilePath ?? ""), "FilePath");

        if (pdfFile != null && pdfFile.Length > 0)
        {
            var stream = pdfFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            form.Add(fileContent, "pdfFile", pdfFile.FileName);
        }

        return form;
    }
}