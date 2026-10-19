using GLMS_API.Models;

namespace GLMS_API.Factories
{
    // Defines factory methods for creating the Contract objects 
    public interface IContractFactory
    {
        Contract CreateDraftContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate);
        Contract CreateActiveContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate);
        Contract CreateOnHoldContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate);
        Contract CreateExpiredContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate);

    }
}