using GLMS_API.Models;
using GLMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_API.Controllers;

//Code Attribution for role-based authorization
//Link:
//https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-9.0

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientsController(ClientService clientService)
    {
        _clientService = clientService;
    }

    // GET: api/clients
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _clientService.GetAllClientsAsync();
        return Ok(clients);
    }

    // GET: api/clients/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await _clientService.GetClientByIdAsync(id);

        if (client == null)
            return NotFound();

        return Ok(client);
    }

    // POST: api/clients
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateClient([FromBody] Client client)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _clientService.AddClientWithUserAsync(client);

        return CreatedAtAction(
            nameof(GetClient),
            new { id = client.Id },
            new
            {
                client,
                username = result.Username,
                temporaryPassword = result.TemporaryPassword
            });
    }

    // PUT: api/clients/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] Client client)
    {
        if (id != client.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _clientService.UpdateClientAsync(client);

        return NoContent();
    }

    // DELETE: api/clients/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        if (await _clientService.ClientHasContractsAsync(id))
        {
            return Conflict(new
            {
                message = "Cannot delete client: they have linked contracts."
            });
        }

        var deleted = await _clientService.DeleteClientAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}