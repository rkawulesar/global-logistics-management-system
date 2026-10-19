using GLMS_Web.ApiServices;
using GLMS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS_Web.Controllers
{
    public class ServiceRequestController : BaseController
    {
        // API service used for frontend-to-backend communication
        private readonly ServiceRequestApiService _serviceRequestApiService;

        public ServiceRequestController(ServiceRequestApiService serviceRequestApiService)
        {
            _serviceRequestApiService = serviceRequestApiService;
        }

        // GET: ServiceRequest
        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var serviceRequests =
                await _serviceRequestApiService.GetServiceRequestsAsync();

            return View(serviceRequests);
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var serviceRequest =
                await _serviceRequestApiService.GetServiceRequestByIdAsync(id.Value);

            if (serviceRequest == null)
                return NotFound();

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        public async Task<IActionResult> Create()
        {
            await LoadContractDropdown();
            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ContractId,Description,Cost,UsdAmount,Status")] ServiceRequest serviceRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadContractDropdown(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            var created =
                await _serviceRequestApiService.CreateServiceRequestAsync(serviceRequest);

            if (!created)
            {
                ModelState.AddModelError(
                    "ContractId",
                    "Service request could not be created. Check that the contract is Active and does not already have a request.");

                await LoadContractDropdown(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var serviceRequest =
                await _serviceRequestApiService.GetServiceRequestByIdAsync(id.Value);

            if (serviceRequest == null)
                return NotFound();

            await LoadContractDropdown(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // POST: ServiceRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,Cost,UsdAmount,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadContractDropdown(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            var updateResult = await _serviceRequestApiService.UpdateServiceRequestAsync(serviceRequest);

            if (updateResult == null)
            {
                ModelState.AddModelError(
                    "",
                    "Service request could not be updated through the API.");

                await LoadContractDropdown(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            if (updateResult.Notifications.Any())
            {
                TempData["ObserverNotifications"] =
                    System.Text.Json.JsonSerializer.Serialize(updateResult.Notifications);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceRequest/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var serviceRequest =
                await _serviceRequestApiService.GetServiceRequestByIdAsync(id.Value);

            if (serviceRequest == null)
                return NotFound();

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted =
                await _serviceRequestApiService.DeleteServiceRequestAsync(id);

            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // Loads contracts from the API into the dropdown list
        private async Task LoadContractDropdown(int? selectedContractId = null)
        {
            var contracts = await _serviceRequestApiService.GetContractDropdownAsync();

            ViewData["ContractId"] = new SelectList(contracts, "Id", "DisplayText", selectedContractId);
        }

        // Retrieves the live USD to ZAR exchange rate from the API
        [HttpGet]
        public async Task<IActionResult> GetUsdToZarRate()
        {
            var rate = await _serviceRequestApiService.GetUsdToZarRateAsync();

            if (rate == null)
                return StatusCode(500, "Currency API unavailable");

            return Json(rate);
        }

        // GET: ServiceRequest/MyServiceRequests
        public async Task<IActionResult> MyServiceRequests()
        {
            var serviceRequests = await _serviceRequestApiService.GetMyServiceRequestsAsync();

            return View("Index", serviceRequests);
        }

    }
}