using GLMS_API.Models;
using GLMS_API.Services;
using GLMS_API.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_API.Controllers;
//Code Attribution for ASP.NET Core Web API CRUD methods
//Link: https://medium.com/@adeebabu655/a-comprehensive-guide-to-c-net-web-api-get-post-put-delete-methods-40e60aff91be
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly ServiceRequestService _serviceRequestService;
    private readonly CurrencyService _currencyService;
    private readonly PricingContext _pricingContext;
    private readonly ServiceRequestSubject _subject;

    public ServiceRequestsController(
        ServiceRequestService serviceRequestService,
        CurrencyService currencyService,
        PricingContext pricingContext,
        ServiceRequestSubject subject)
    {
        _serviceRequestService = serviceRequestService;
        _currencyService = currencyService;
        _pricingContext = pricingContext;
        _subject = subject;
    }
    // GET: api/servicerequests
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetServiceRequests()
    {
        var serviceRequests =
            await _serviceRequestService.GetAllServiceRequestsAsync();

        return Ok(serviceRequests);
    }
    // GET: api/servicerequests
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> GetServiceRequest(int id)
    {
        var serviceRequest = await _serviceRequestService.GetServiceRequestByIdAsync(id);

        if (serviceRequest == null)
            return NotFound();

        // Client can only view their own service requests
        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            if (serviceRequest.Contract?.ClientId.ToString() != clientIdClaim)
                return Forbid();
        }

        return Ok(serviceRequest);
    }

    // POST: api/servicerequests
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequest serviceRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _serviceRequestService.AddServiceRequestWithCurrencyAsync(
            serviceRequest, _currencyService, _pricingContext);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetServiceRequest),
            new { id = serviceRequest.Id }, serviceRequest);
    }

    // PUT: api/servicerequests/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateServiceRequest(int id, [FromBody] ServiceRequest serviceRequest)
    {
        // Ensure route ID matches body ID
        if (id != serviceRequest.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Delegates all logic to the service layer
        var result = await _serviceRequestService.UpdateServiceRequestWithCurrencyAsync(
            id, serviceRequest, _currencyService, _pricingContext);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Service request not found.")
                return NotFound();

            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new
        {
            serviceRequest = result.ServiceRequest,
            notifications = result.Notifications
        });
    }

    // DELETE: api/servicerequests/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var serviceRequest = await _serviceRequestService.GetServiceRequestByIdAsync(id);

        if (serviceRequest == null)
            return NotFound();

        await _serviceRequestService.DeleteServiceRequestAsync(serviceRequest);

        return NoContent();
    }

    // GET: api/servicerequests/my
    // Returns service requests linked to the logged-in client's contracts
    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyServiceRequests()
    {
        var clientIdClaim = User.FindFirst("clientId")?.Value;

        if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out int clientId))
            return Forbid();

        var serviceRequests = await _serviceRequestService
            .GetServiceRequestsByClientIdAsync(clientId);

        return Ok(serviceRequests);
    }

    // GET: api/servicerequests/contracts/dropdown
    [HttpGet("contracts/dropdown")]
    public async Task<IActionResult> GetContractDropdown()
    {
        var contracts = await _serviceRequestService.GetContractDropdownAsync();
        return Ok(contracts);
    }

    // GET: api/servicerequests/rate/usd-zar
    [HttpGet("rate/usd-zar")]
    public async Task<IActionResult> GetUsdToZarRate()
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();

        if (rate == null)
            return StatusCode(500, new { message = "Currency API unavailable." });

        return Ok(rate);
    }
}