using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;

namespace GLMS_Web.Tests.IntegrationTests
{
    //Code Attribution for API integration testing using HttpClient and NUnit
    //Links:
    //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
    //https://docs.nunit.org/articles/nunit/writing-tests/attributes.html
    //https://docs.nunit.org/articles/nunit/writing-tests/assertions/assertions.html
    [TestFixture]
    public class ApiIntegrationTests
    {
        private HttpClient _client = null!;

        private const string ApiBaseUrl = "https://localhost:7134/api/";

        [SetUp]
        public void Setup()
        {
            // Creates HttpClient for calling the API
            _client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl)
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Cleans up HttpClient after each test
            _client.Dispose();
        }

        // Logs into the API and stores the JWT token
        private async Task LoginAsAdminAsync()
        {
            var loginRequest = new
            {
                Username = Environment.GetEnvironmentVariable("GLMS_TEST_ADMIN_USERNAME")
                    ?? throw new InvalidOperationException("Set GLMS_TEST_ADMIN_USERNAME before running authenticated integration tests."),
                Password = Environment.GetEnvironmentVariable("GLMS_TEST_ADMIN_PASSWORD")
                    ?? throw new InvalidOperationException("Set GLMS_TEST_ADMIN_PASSWORD before running authenticated integration tests.")
            };

            // Sends login request to the API
            var response =
                await _client.PostAsJsonAsync(
                    "Auth/login",
                    loginRequest);

            // Verifies login succeeded
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            var loginResponse =
                await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

            Assert.That(loginResponse, Is.Not.Null);
            Assert.That(loginResponse!.Token, Is.Not.Empty);

            // Adds JWT token to future API requests
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    loginResponse.Token);
        }

        // Adds a fake PDF file to the multipart form
        private void AddFakePdfFile(MultipartFormDataContent form)
        {
            var fileBytes = new byte[] { 37, 80, 68, 70 };
            var fileContent = new ByteArrayContent(fileBytes);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            form.Add(fileContent, "pdfFile", "test-contract.pdf");
        }

        // Tests GET: api/contracts returns 200 and valid JSON array
        [Test]
        public async Task GetContracts_ReturnsOk()
        {
            await LoginAsAdminAsync();

            // Calls contracts endpoint
            var response =
                await _client.GetAsync("Contracts");

            // Verifies successful response
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            // Deserialises and verifies returned data shape
            var contracts =
                await response.Content
                    .ReadFromJsonAsync<List<ContractDto>>();

            Assert.That(contracts, Is.Not.Null);
        }

        // Tests GET: api/clients returns 200 and valid JSON array
        [Test]
        public async Task GetClients_ReturnsOk()
        {
            await LoginAsAdminAsync();

            // Calls clients endpoint
            var response =
                await _client.GetAsync("Clients");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            // Deserialises and verifies returned data shape
            var clients =
                await response.Content
                    .ReadFromJsonAsync<List<ClientDto>>();

            Assert.That(clients, Is.Not.Null);
        }

        // Tests GET: api/servicerequests returns 200
        [Test]
        public async Task GetServiceRequests_ReturnsOk()
        {
            await LoginAsAdminAsync();

            // Calls service requests endpoint
            var response =
                await _client.GetAsync("ServiceRequests");

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            // Deserialises and verifies returned data shape
            var requests =
                await response.Content
                    .ReadFromJsonAsync<List<ServiceRequestDto>>();

            Assert.That(requests, Is.Not.Null);
        }

        // Tests that an invalid contract ID returns 403 or 404
        [Test]
        public async Task GetContract_InvalidId_ReturnsError()
        {
            await LoginAsAdminAsync();

            // Sends request with invalid contract ID
            var response =
                await _client.GetAsync("Contracts/999999");

            // Verifies that the API handles invalid IDs correctly
            Assert.That(
                response.StatusCode,
                Is.AnyOf(
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.NotFound));
        }

        // Tests that unauthenticated requests are rejected
        [Test]
        public async Task GetContracts_NoToken_ReturnsUnauthorized()
        {
            // Calls endpoint without logging in first
            var response =
                await _client.GetAsync("Contracts");

            // Verifies unauthorized response
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        // Tests full data integrity: Create → Read → Delete
        [Test]
        public async Task CreateAndReadContract_Success()
        {
            await LoginAsAdminAsync();

            var serviceLevel = "IntegrationTest";

            // Builds multipart form matching the API's [FromForm] endpoint
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("1"), "ClientId");
            form.Add(new StringContent(DateTime.Today.ToString("o")), "StartDate");
            form.Add(new StringContent(DateTime.Today.AddMonths(6).ToString("o")), "EndDate");
            form.Add(new StringContent("Draft"), "Status");
            form.Add(new StringContent(serviceLevel), "ServiceLevel");

            // Adds fake PDF because API requires a PDF on create
            AddFakePdfFile(form);

            // Sends POST request to create contract
            var createResponse =
                await _client.PostAsync("Contracts", form);

            // Verifies contract creation returned 201 Created
            Assert.That(
                createResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.Created));

            // Retrieves all contracts and deserialises
            var getResponse =
                await _client.GetAsync("Contracts");

            Assert.That(
                getResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));

            var contracts =
                await getResponse.Content
                    .ReadFromJsonAsync<List<ContractDto>>();

            // Verifies created contract exists in the returned list
            Assert.That(contracts, Is.Not.Null);
            Assert.That(
                contracts!.Any(c => c.ServiceLevel == serviceLevel),
                Is.True);

            // Finds created contract for cleanup
            var createdContract =
                contracts.LastOrDefault(c =>
                    c.ServiceLevel == serviceLevel);

            // Deletes test contract from database
            if (createdContract != null)
            {
                var deleteResponse =
                    await _client.DeleteAsync(
                        $"Contracts/{createdContract.Id}");

                // Verifies deletion returned 204 No Content
                Assert.That(
                    deleteResponse.StatusCode,
                    Is.EqualTo(HttpStatusCode.NoContent));

                // Verifies contract no longer exists
                var checkResponse =
                    await _client.GetAsync(
                        $"Contracts/{createdContract.Id}");

                Assert.That(
                    checkResponse.StatusCode,
                    Is.AnyOf(
                        HttpStatusCode.Forbidden,
                        HttpStatusCode.NotFound));
            }
        }

        // Tests invalid contract creation with no selected client
        [Test]
        public async Task CreateContract_NoClient_ReturnsBadRequest()
        {
            await LoginAsAdminAsync();

            // Builds form with invalid ClientId of 0
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent("0"), "ClientId");
            form.Add(new StringContent(DateTime.Today.ToString("o")), "StartDate");
            form.Add(new StringContent(DateTime.Today.AddMonths(6).ToString("o")), "EndDate");
            form.Add(new StringContent("Draft"), "Status");
            form.Add(new StringContent("Standard"), "ServiceLevel");

            // Adds fake PDF so only the ClientId validation fails
            AddFakePdfFile(form);

            // Sends POST request with invalid data
            var response =
                await _client.PostAsync("Contracts", form);

            // Verifies bad request is returned
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));
        }

        // Stores login response data
        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;

            public string Role { get; set; } = string.Empty;
        }

        // Stores contract response data
        private class ContractDto
        {
            public int Id { get; set; }

            public string ServiceLevel { get; set; } = string.Empty;
        }

        // Stores client response data
        private class ClientDto
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = string.Empty;
        }

        // Stores service request response data
        private class ServiceRequestDto
        {
            public int Id { get; set; }

            public string Description { get; set; } = string.Empty;
        }
    }
}