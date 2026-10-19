using GLMS_API.Models;

namespace GLMS_API.Factories
{
    // Code Attribution for using Factory Method
    // Link: https://refactoring.guru/design-patterns/factory-method
    public class ContractFactory : IContractFactory
    {
        // Creates a draft contract (initial state)
        public Contract CreateDraftContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate)
        {
            return new Contract
            {
                ClientId = clientId,
                ServiceLevel = serviceLevel,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Draft"
            };
        }

        // Creates an active contract with validation
        public Contract CreateActiveContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate)
        {
            // Prevent invalid active contracts
            if (endDate <= DateTime.Now)
                throw new InvalidOperationException("Cannot create an Active contract with a past end date.");

            return new Contract
            {
                ClientId = clientId,
                ServiceLevel = serviceLevel,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active"
            };
        }

        // Creates an expired contract
        public Contract CreateExpiredContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate)
        {
            return new Contract
            {
                ClientId = clientId,
                ServiceLevel = serviceLevel,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Expired"
            };
        }

        // Creates a contract on hold
        public Contract CreateOnHoldContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate)
        {
            return new Contract
            {
                ClientId = clientId,
                ServiceLevel = serviceLevel,
                StartDate = startDate,
                EndDate = endDate,
                Status = "On Hold"
            };
        }
    }
}