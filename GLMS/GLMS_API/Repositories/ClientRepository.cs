using GLMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS_API.Repositories
{
    // Handles database operations for Client entity
    public class ClientRepository : IClientRepository
    {
        private readonly GlmsContext _context;

        public ClientRepository(GlmsContext context)
        {
            _context = context;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return false;
            }

            // Deletes linked user account first
            var linkedUsers = await _context.Users
                .Where(u => u.ClientId == id)
                .ToListAsync();

            if (linkedUsers.Any())
            {
                _context.Users.RemoveRange(linkedUsers);
            }

            _context.Clients.Remove(client);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clients.AnyAsync(c => c.Id == id);
        }
        public async Task<bool> HasContractsAsync(int clientId)
        {
            return await _context.Contracts.AnyAsync(c => c.ClientId == clientId);
        }
    }
}