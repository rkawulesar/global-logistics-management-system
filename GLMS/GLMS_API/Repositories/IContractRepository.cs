using GLMS_API.Models;

namespace GLMS_API.Repositories
{
    //Code Attribution for Repository Pattern
    //Link: https://code-maze.com/net-core-web-development-part4/

    // Repository interface for Contract operations
    public interface IContractRepository
    {
        Task<List<Contract>> GetAllAsync();
        Task<Contract?> GetByIdAsync(int id);
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasServiceRequestsAsync(int contractId);
    }
}
