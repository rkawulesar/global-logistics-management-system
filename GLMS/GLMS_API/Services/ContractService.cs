using System.Security.Claims;
using GLMS_API.Factories;
using GLMS_API.Models;
using GLMS_API.Repositories;

namespace GLMS_API.Services
{
    // Handles business logic for contracts
    public class ContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractFactory _contractFactory;

        public ContractService(IContractRepository contractRepository, IContractFactory contractFactory)
        {
            _contractRepository = contractRepository;
            _contractFactory = contractFactory;
        }

        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await _contractRepository.GetAllAsync();
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            return await _contractRepository.GetByIdAsync(id);
        }

        // Validates contract details and uploaded file
        public async Task<(bool Success, string? ErrorMessage)> ValidateContractAsync(Contract contract, IFormFile? pdfFile, bool isCreate)
        {
            // Ensures end date is after start date
            if (contract.EndDate < contract.StartDate)
            {
                return (false, "End date cannot be earlier than start date.");
            }

            // Requires a signed PDF when creating a new contract
            if (isCreate)
            {
                if (pdfFile == null || pdfFile.Length == 0)
                {
                    return (false, "Please upload a signed agreement PDF.");
                }
            }

            if (pdfFile != null && pdfFile.Length > 0)
            {
                // Restricts file size to 10MB
                if (pdfFile.Length > 10 * 1024 * 1024)
                {
                    return (false, "File size cannot exceed 10 MB.");
                }

                // Accepts only PDF files
                var ext = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    return (false, "Only PDF files are allowed.");
                }
            }

            return (true, null);
        }

        public async Task<Contract> AddContractAsync(Contract contract)
        {
            //The Factory creates the correct contract type based on status
            Contract newContract = contract.Status switch
            {
                "Active" => _contractFactory.CreateActiveContract(
                                contract.ClientId, contract.ServiceLevel,
                                contract.StartDate, contract.EndDate),

                "Expired" => _contractFactory.CreateExpiredContract(
                                contract.ClientId, contract.ServiceLevel,
                                contract.StartDate, contract.EndDate),

                "On Hold" => _contractFactory.CreateOnHoldContract(
                                contract.ClientId, contract.ServiceLevel,
                                contract.StartDate, contract.EndDate),

                _ => _contractFactory.CreateDraftContract(
                                contract.ClientId, contract.ServiceLevel,
                                contract.StartDate, contract.EndDate)
            };

            //Keeps the uploaded file path from the controller
            newContract.FilePath = contract.FilePath;

            await _contractRepository.AddAsync(newContract);
            return newContract;
        }

        public async Task UpdateContractAsync(Contract contract)
        {
            await _contractRepository.UpdateAsync(contract);
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
            {
                return false;
            }

            //Delete the physical PDF file if it exists
            if (!string.IsNullOrEmpty(contract.FilePath))
            {
                string filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    contract.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return await _contractRepository.DeleteAsync(id);
        }

        public async Task<bool> ContractExistsAsync(int id)
        {
            return await _contractRepository.ExistsAsync(id);
        }

        // Prevents deletion when linked service requests exist
        public async Task<bool> ContractHasServiceRequestsAsync(int id)
        {
            return await _contractRepository.HasServiceRequestsAsync(id);
        }

        //Gets the filtered contracts
        public async Task<List<Contract>> GetFilteredContractsAsync(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            var contracts = await _contractRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(status))
                contracts = contracts.Where(c => c.Status == status).ToList();

            if (!string.IsNullOrEmpty(serviceLevel))
                contracts = contracts.Where(c => c.ServiceLevel == serviceLevel).ToList();

            if (startDate.HasValue)
                contracts = contracts.Where(c => c.StartDate.Date >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                contracts = contracts.Where(c => c.EndDate.Date <= endDate.Value.Date).ToList();

            return contracts;
        }

        public async Task<List<Contract>> GetContractsByClientIdAsync(int clientId)
        {
            var all = await _contractRepository.GetAllAsync();
            return all.Where(c => c.ClientId == clientId).ToList();
        }

        public async Task<(bool Success, Contract? Contract)> GetAuthorizedContractAsync(int contractId, ClaimsPrincipal user)
        {
            var contract = await GetContractByIdAsync(contractId);

            if (contract == null)
                return (false, null);

            // Admin can access everything
            if (user.IsInRole("Admin"))
                return (true, contract);

            // Client can only access own contracts
            if (user.IsInRole("Client"))
            {
                var clientIdClaim =
                    user.FindFirst("clientId")?.Value;

                if (string.IsNullOrEmpty(clientIdClaim) ||
                    !int.TryParse(clientIdClaim, out int clientId))
                {
                    return (false, null);
                }

                if (contract.ClientId != clientId)
                    return (false, null);

                return (true, contract);
            }

            return (false, null);
        }

        public async Task<List<Contract>> GetFilteredContractsByClientIdAsync(int clientId, string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            var contracts = await GetContractsByClientIdAsync(clientId);

            if (!string.IsNullOrEmpty(status))
                contracts = contracts.Where(c => c.Status == status).ToList();

            if (!string.IsNullOrEmpty(serviceLevel))
                contracts = contracts.Where(c => c.ServiceLevel == serviceLevel).ToList();

            if (startDate.HasValue)
                contracts = contracts.Where(c => c.StartDate.Date >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                contracts = contracts.Where(c => c.EndDate.Date <= endDate.Value.Date).ToList();

            return contracts;
        }

        //Code Attribution for Uploading a file
        //Link: https://www.c-sharpcorner.com/article/complete-file-upload-and-download-in-asp-net-core-mvc/
        // Validates contract and saves the uploaded file to the server
        public async Task<(bool Success, string? ErrorMessage)> ValidateAndSaveFileAsync(
            Contract contract, IFormFile? pdfFile, bool isCreate)
        {
            // Reuses existing validation logic
            var validation = await ValidateContractAsync(contract, pdfFile, isCreate);
            if (!validation.Success)
                return validation;

            // Saves the file if one was provided
            if (pdfFile != null && pdfFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "files");

                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}.pdf";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await pdfFile.CopyToAsync(stream);

                contract.FilePath = "/files/" + fileName;
            }

            return (true, null);
        }
        public string? GetPdfFilePath(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);

            return File.Exists(filePath) ? filePath : null;
        }
    }
}