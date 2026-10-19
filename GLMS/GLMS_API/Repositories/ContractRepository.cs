using GLMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS_API.Repositories
{
    //Code Attribution for Repository Pattern
    //Link: https://code-maze.com/net-core-web-development-part4/

    // Handles database operations for Contract entity
    public class ContractRepository : IContractRepository
    {
        private readonly GlmsContext _context;

        public ContractRepository(GlmsContext context)
        {
            _context = context;
        }

        public async Task<List<Contract>> GetAllAsync()
        {
            return await _context.Contracts
                .AsNoTracking()
                .Include(c => c.Client)
                .ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .AsNoTracking()
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Contract contract)
        {
            var existing = await _context.Contracts.FindAsync(contract.Id);

            if (existing == null) return;

            // Maps updated values onto the tracked entity
            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.Status = contract.Status;
            existing.ServiceLevel = contract.ServiceLevel;

            if (!string.IsNullOrEmpty(contract.FilePath))
                existing.FilePath = contract.FilePath;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return false;
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(c => c.Id == id);
        }
        public async Task<bool> HasServiceRequestsAsync(int contractId)
        {
            return await _context.ServiceRequests.AnyAsync(s => s.ContractId == contractId);
        }
    }
}
