using System.Configuration;
using GLMS_Web.ApiServices;
using GLMS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS_Web.Controllers
{
    public class ContractController : BaseController
    {
        // API Services
        private readonly ContractApiService _contractApiService;
        private readonly ClientApiService _clientApiService;
        private readonly IConfiguration _configuration;

        public ContractController(
            ContractApiService contractApiService, ClientApiService clientApiService, IConfiguration configuration)
        {
            _contractApiService = contractApiService;
            _clientApiService = clientApiService;
            _configuration = configuration;
        }

        // GET: Contract
        public async Task<IActionResult> Index(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            // Sends filter values to the API
            var contracts = await _contractApiService.GetContractsAsync(status, serviceLevel, startDate, endDate);

            // Strips /api/ so files are served from the API root
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:PublicBaseUrl"]?.TrimEnd('/');

            ViewBag.ServiceLevel = serviceLevel;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(contracts);
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieves contract from API
            var contract = await _contractApiService.GetContractByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:PublicBaseUrl"]?.TrimEnd('/');

            return View(contract);
        }

        // GET: Contract/Create
        public async Task<IActionResult> Create()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Admin")
            {
                var clients = await _clientApiService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(clients, "Id", "FullName");
            }

            return View();
        }

        // POST: Contract/Create

        //Code Attribution for Uploading a file
        //Link: https://www.c-sharpcorner.com/article/complete-file-upload-and-download-in-asp-net-core-mvc/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile? pdfFile)
        {
            // Sends contract and PDF file to the API.
            // Validation and file saving are handled by the backend API.
            var created =
                await _contractApiService.CreateContractAsync(contract, pdfFile);

            if (!created)
            {
                ModelState.AddModelError(
                    "",
                    "Failed to create contract. Please check that the file is a PDF and the dates are valid.");

                var clients = await _clientApiService.GetClientsAsync();

                ViewData["ClientId"] =
                    new SelectList(
                        clients,
                        "Id",
                        "FullName",
                        contract.ClientId);

                return View(contract);
            }

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Client")
            {
                return RedirectToAction(nameof(MyContracts));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract =
                await _contractApiService.GetContractByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:PublicBaseUrl"]?.TrimEnd('/');

            // Only Admin needs the client dropdown
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Admin")
            {
                var clients = await _clientApiService.GetClientsAsync();
                ViewData["ClientId"] = new SelectList(
                    clients, "Id", "FullName", contract.ClientId);
            }

            return View(contract);
        }
        // POST: Contract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,FilePath")] Contract contract, IFormFile? pdfFile)
        {
            // Ensures route id matches contract id
            if (id != contract.Id)
            {
                return NotFound();
            }

            // Sends update request to API service.
            // Validation and PDF replacement are handled in ContractApiService.
            var updated = await _contractApiService.UpdateContractAsync(contract, pdfFile);

            if (!updated)
            {
                ModelState.AddModelError("", "Failed to update contract. Please check that the file is a PDF and the dates are valid.");

                var clients = await _clientApiService.GetClientsAsync();

                ViewData["ClientId"] = new SelectList(clients, "Id", "FullName", contract.ClientId);

                return View(contract);
            }

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Client")
            {
                return RedirectToAction(nameof(MyContracts));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contract/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieves contract from API
            var contract =
                await _contractApiService.GetContractByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Sends delete request to API
            var deleted =
                await _contractApiService.DeleteContractAsync(id);

            if (!deleted)
            {
                ViewBag.ErrorMessage =
                    "Cannot delete this contract because it has linked service requests.";

                var contract =
                    await _contractApiService.GetContractByIdAsync(id);

                return View("Delete", contract);
            }

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Client")
            {
                return RedirectToAction(nameof(MyContracts));
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Contract/MyContracts
        public async Task<IActionResult> MyContracts(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            var contracts = await _contractApiService.GetMyContractsAsync(status, serviceLevel, startDate, endDate);

            ViewBag.ApiBaseUrl = _configuration["ApiSettings:PublicBaseUrl"]?.TrimEnd('/');
            ViewBag.ServiceLevel = serviceLevel;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View("Index", contracts);
        }
    }
}