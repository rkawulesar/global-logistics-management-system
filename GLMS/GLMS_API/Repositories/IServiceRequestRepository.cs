using GLMS_API.Models;

namespace GLMS_API.Repositories
{    //Code Attribution for Repository Pattern
    //Link: https://code-maze.com/net-core-web-development-part4/

    // Repository interface for ServiceRequest operations
    public interface IServiceRequestRepository
    {
        Task<List<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task AddAsync(ServiceRequest serviceRequest);
        Task UpdateAsync(ServiceRequest serviceRequest);
        Task DeleteAsync(ServiceRequest serviceRequest);
        Task<bool> ExistsAsync(int id);
        Task<Contract?> GetContractByIdAsync(int contractId);
        Task<List<ContractDropdownItem>> GetContractDropdownAsync();
        Task<bool> ContractAlreadyHasRequestAsync(int contractId);
    }
}