
using GLMS_API.Models;

namespace GLMS_API.Repositories
{
    // Repository interface for Client operations
    public interface IClientRepository
    {
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasContractsAsync(int clientId);
    }
}