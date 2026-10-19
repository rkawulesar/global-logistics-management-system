using GLMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS_API.Repositories
{    //Code Attribution for Repository Pattern
     //Link: https://code-maze.com/net-core-web-development-part4/

    // Handles database operations for ServiceRequest entity
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly GlmsContext _context;

        public ServiceRequestRepository(GlmsContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceRequest>> GetAllAsync()
        {
            return await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _context.ServiceRequests
             .AsNoTracking()
            .Include(s => s.Contract)
            .ThenInclude(c => c.Client)
             .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Update(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ServiceRequests.AnyAsync(s => s.Id == id);
        }

        public async Task<Contract?> GetContractByIdAsync(int contractId)
        {
            return await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }

        // Gets contracts for dropdown display
        public async Task<List<ContractDropdownItem>> GetContractDropdownAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Select(c => new ContractDropdownItem
                {
                    Id = c.Id,
                    DisplayText = c.Client.FirstName + " " + c.Client.Surname + " - " + c.ServiceLevel + " (" + c.Status + ")"
                })
                .ToListAsync();
        }

        public async Task<bool> ContractAlreadyHasRequestAsync(int contractId)
        {
            return await _context.ServiceRequests.AnyAsync(s => s.ContractId == contractId);
        }
    }
}