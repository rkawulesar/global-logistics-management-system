using GLMS_API.Models;
using GLMS_API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GLMS_API.Services;

// Handles business logic for Client operations
public class ClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly GlmsContext _context;

    public ClientService(
        IClientRepository clientRepository,
        GlmsContext context)
    {
        _clientRepository = clientRepository;
        _context = context;
    }

    public async Task<List<Client>> GetAllClientsAsync()
    {
        return await _clientRepository.GetAllAsync();
    }

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        return await _clientRepository.GetByIdAsync(id);
    }

    public async Task AddClientAsync(Client client)
    {
        await _clientRepository.AddAsync(client);
    }

    public async Task UpdateClientAsync(Client client)
    {
        await _clientRepository.UpdateAsync(client);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        return await _clientRepository.DeleteAsync(id);
    }

    public async Task<bool> ClientExistsAsync(int id)
    {
        return await _clientRepository.ExistsAsync(id);
    }
    public async Task<bool> ClientHasContractsAsync(int id)
    {
        return await _clientRepository.HasContractsAsync(id);
    }

    public async Task<(bool Success, string Username, string TemporaryPassword)>
    AddClientWithUserAsync(Client client)
    {
        await AddClientAsync(client);

        var temporaryPassword =
            $"Client{Guid.NewGuid().ToString("N")[..6]}!";

        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            Username = client.Email,
            Role = "Client",
            ClientId = client.Id
        };

        user.Password = hasher.HashPassword(user, temporaryPassword);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return (true, client.Email, temporaryPassword);
    }

}