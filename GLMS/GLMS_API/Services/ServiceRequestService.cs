using GLMS_API.Models;
using GLMS_API.Repositories;
using GLMS_API.Strategies;
namespace GLMS_API.Services
{
    public class ServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly ServiceRequestSubject _subject;

        public ServiceRequestService(IServiceRequestRepository serviceRequestRepository, ServiceRequestSubject subject)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _subject = subject;
        }


        public async Task<List<ServiceRequest>> GetAllServiceRequestsAsync()
        {
            return await _serviceRequestRepository.GetAllAsync();
        }

        // Get a single service request by ID
        public async Task<ServiceRequest?> GetServiceRequestByIdAsync(int id)
        {
            return await _serviceRequestRepository.GetByIdAsync(id);
        }

        // Add a new service request with validation
        public async Task<(bool Success, string? ErrorMessage)> AddServiceRequestAsync(ServiceRequest serviceRequest)
        {
            // Check if contract exists
            var contract = await _serviceRequestRepository.GetContractByIdAsync(serviceRequest.ContractId);

            if (contract == null)
            {
                return (false, "Invalid contract selected.");
            }

            // Prevent requests on invalid contract statuses
            if (contract.Status == "Expired" || contract.Status == "On Hold")
            {
                return (false, "Cannot create a service request for an Expired or On Hold contract.");
            }

            // Ensure only one request per contract
            if (await _serviceRequestRepository.ContractAlreadyHasRequestAsync(serviceRequest.ContractId))
            {
                return (false, "This contract already has a service request.");
            }


            await _serviceRequestRepository.AddAsync(serviceRequest);
            return (true, null);
        }

        // Update an existing service request
        public async Task<(bool Success, string? ErrorMessage)> UpdateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            // Validate contract
            var contract = await _serviceRequestRepository.GetContractByIdAsync(serviceRequest.ContractId);

            if (contract == null)
            {
                return (false, "Invalid contract selected.");
            }

            // Prevent updates on invalid contract statuses
            if (contract.Status == "Expired" || contract.Status == "On Hold")
            {
                return (false, "Cannot assign a service request to an Expired or On Hold contract.");
            }


            var existingRequest = await _serviceRequestRepository.GetByIdAsync(serviceRequest.Id);

            if (existingRequest == null)
            {
                return (false, "Service request not found.");
            }

            // Check if status changed
            bool statusChanged = existingRequest.Status != serviceRequest.Status;


            await _serviceRequestRepository.UpdateAsync(serviceRequest);

            // Notify observers if status changed
            if (statusChanged)
            {
                existingRequest.Status = serviceRequest.Status;
                existingRequest.Description = serviceRequest.Description;

                _subject.NotifyObservers(existingRequest);
            }

            return (true, null);
        }

        // Delete a service request
        public async Task DeleteServiceRequestAsync(ServiceRequest serviceRequest)
        {
            await _serviceRequestRepository.DeleteAsync(serviceRequest);
        }

        // Check if service request exists
        public async Task<bool> ServiceRequestExistsAsync(int id)
        {
            return await _serviceRequestRepository.ExistsAsync(id);
        }

        // Get contracts for dropdown list 
        public async Task<List<ContractDropdownItem>> GetContractDropdownAsync()
        {
            return await _serviceRequestRepository.GetContractDropdownAsync();
        }

        // Handles currency conversion before adding a service request
        public async Task<(bool Success, string? ErrorMessage)> AddServiceRequestWithCurrencyAsync(
            ServiceRequest serviceRequest,
            CurrencyService currencyService,
            PricingContext pricingContext)
        {
            if (!serviceRequest.UsdAmount.HasValue)
                return (false, "Please enter an amount in USD.");

            var rate = await currencyService.GetUsdToZarRateAsync();
            if (rate == null)
                return (false, "Currency API is currently unavailable.");

            serviceRequest.Cost = pricingContext.CalculatePrice(
                serviceRequest.UsdAmount.Value, rate.Value);

            return await AddServiceRequestAsync(serviceRequest);
        }

        // Updates a service request, handling currency conversion and observer notifications
        public async Task<(bool Success, string? ErrorMessage, ServiceRequest? ServiceRequest, List<string> Notifications)>
            UpdateServiceRequestWithCurrencyAsync(int id, ServiceRequest serviceRequest, CurrencyService currencyService, PricingContext pricingContext)
        {
            var existingRequest = await GetServiceRequestByIdAsync(id);
            if (existingRequest == null)
                return (false, "Service request not found.", null, new List<string>());

            serviceRequest.ContractId = existingRequest.ContractId;

            // Recalculates the ZAR cost if a new USD amount was provided
            if (serviceRequest.UsdAmount.HasValue && serviceRequest.UsdAmount.Value > 0)
            {
                var rate = await currencyService.GetUsdToZarRateAsync();
                if (rate == null)
                    return (false, "Currency API is currently unavailable.", null, new List<string>());

                serviceRequest.Cost = pricingContext.CalculatePrice(
                    serviceRequest.UsdAmount.Value, rate.Value);
            }

            var result = await UpdateServiceRequestAsync(serviceRequest);
            if (!result.Success)
                return (false, result.ErrorMessage, null, new List<string>());


            return (true, null, serviceRequest, _subject.Notifications.ToList());
        }

        // Gets service requests by finding contracts belonging to the client first
        public async Task<List<ServiceRequest>> GetServiceRequestsByClientIdAsync(int clientId)
        {
            var all = await _serviceRequestRepository.GetAllAsync();
            return all
                .Where(sr => sr.Contract != null && sr.Contract.ClientId == clientId)
                .ToList();
        }
    }
}