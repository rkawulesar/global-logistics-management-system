using GLMS_API.Models;
using GLMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly ContractService _contractService;

    public ContractsController(ContractService contractService)
    {
        _contractService = contractService;
    }

    // GET: api/contracts
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetContracts(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
    {
        var contracts = await _contractService.GetFilteredContractsAsync(status, serviceLevel, startDate, endDate);

        return Ok(contracts);
    }

    // GET: api/contracts/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContract(int id)
    {
        var result =
            await _contractService.GetAuthorizedContractAsync(
                id,
                User);

        if (!result.Success)
            return Forbid();

        return Ok(result.Contract);
    }

    // POST: api/contracts
    [HttpPost]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> CreateContract([FromForm] Contract contract, IFormFile? pdfFile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (User.IsInRole("Client"))
        {
            // Retrieves ClientId from JWT token
            var clientIdClaim = User.FindFirst("clientId")?.Value;

            if (string.IsNullOrEmpty(clientIdClaim) ||
                !int.TryParse(clientIdClaim, out int clientId))
            {
                return Forbid();
            }

            // Forces the logged-in client's ID
            contract.ClientId = clientId;
        }

        if (User.IsInRole("Admin") && contract.ClientId <= 0)
            return BadRequest("Please select a client.");

        // Validates contract details and saves the uploaded PDF to the server
        var validation = await _contractService.ValidateAndSaveFileAsync(contract, pdfFile, isCreate: true);
        if (!validation.Success)
            return BadRequest(validation.ErrorMessage);

        try
        {
            var saved = await _contractService.AddContractAsync(contract);
            return CreatedAtAction(nameof(GetContract), new { id = saved.Id }, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/contracts/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Client")]
    public async Task<IActionResult> UpdateContract(int id, [FromForm] Contract contract, IFormFile? pdfFile)
    {
        if (id != contract.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _contractService.ContractExistsAsync(id))
            return NotFound();

        // Client can only edit their own contracts
        if (User.IsInRole("Client"))
        {
            var clientIdClaim = User.FindFirst("clientId")?.Value;
            var existing = await _contractService.GetContractByIdAsync(id);
            if (existing?.ClientId.ToString() != clientIdClaim)
                return Forbid();
        }

        // Validates contract details and saves the uploaded PDF to the server
        var validation = await _contractService.ValidateAndSaveFileAsync(contract, pdfFile, isCreate: false);
        if (!validation.Success)
            return BadRequest(validation.ErrorMessage);

        await _contractService.UpdateContractAsync(contract);
        return NoContent();
    }

    // PATCH: api/contracts/5/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateContractStatusDto dto)
    {
        var contract = await _contractService.GetContractByIdAsync(id);

        if (contract == null)
            return NotFound();

        contract.Status = dto.Status;

        await _contractService.UpdateContractAsync(contract);

        return Ok(contract);
    }

    // DELETE: api/contracts/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        if (await _contractService.ContractHasServiceRequestsAsync(id))
        {
            return Conflict(new
            {
                message = "Cannot delete: contract has linked service requests."
            });
        }

        var deleted = await _contractService.DeleteContractAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    // GET: api/contracts/my
    // Returns contracts belonging to the logged-in client
    [HttpGet("my")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyContracts(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
    {
        var clientIdClaim = User.FindFirst("clientId")?.Value;

        if (string.IsNullOrEmpty(clientIdClaim) ||
            !int.TryParse(clientIdClaim, out int clientId))
        {
            return Forbid();
        }

        var contracts = await _contractService.GetFilteredContractsByClientIdAsync(clientId, status, serviceLevel, startDate, endDate);


        return Ok(contracts);
    }

    //Code Attribution for ASP.NET Core AllowAnonymous authorization attribute
    //Link: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.allowanonymousattribute?view=aspnetcore-9.0
    [HttpGet("download/{fileName}")]
    [AllowAnonymous]
    public IActionResult DownloadFile(string fileName)
    {
        // Retrieves the full file path from the service layer
        var filePath = _contractService.GetPdfFilePath(fileName);

        // Returns 404 if the file does not exist
        if (filePath == null)
            return NotFound();

        // Forces the browser to download the PDF file
        return PhysicalFile(filePath, "application/pdf", fileName);
    }
}