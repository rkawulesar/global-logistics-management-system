using GLMS_Web.ApiServices;
using GLMS_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_Web.Controllers
{
    public class ClientController : BaseController
    {
        private readonly ClientApiService _clientApiService;

        public ClientController(ClientApiService clientApiService)
        {
            _clientApiService = clientApiService;
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            // Retrieves clients from the API
            var clients = await _clientApiService.GetClientsAsync();
            return View(clients);
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieves selected client from the API
            var client = await _clientApiService.GetClientByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,Surname,Email,PhoneNumber,Region")] Client client)
        {
            if (!ModelState.IsValid)
            {
                return View(client);
            }

            // Sends new client data to the API
            var created =
                await _clientApiService.CreateClientAsync(client);

            if (created == null)
            {
                ModelState.AddModelError(
                    "",
                    "Failed to create client through the API.");

                return View(client);
            }

            // Stores generated login details
            TempData["LoginDetails"] =
                $"Username: {created.Username} | Temporary Password: {created.TemporaryPassword}";

            return RedirectToAction(nameof(Index));
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieves client from the API for editing
            var client = await _clientApiService.GetClientByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,Surname,Email,PhoneNumber,Region")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            // Sends updated client data to the API
            var updated = await _clientApiService.UpdateClientAsync(client);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Client/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieves client from the API before delete confirmation
            var client = await _clientApiService.GetClientByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Sends delete request to the API
            var deleted = await _clientApiService.DeleteClientAsync(id);

            if (!deleted)
            {
                ViewBag.ErrorMessage = "Cannot delete this client because they may have linked contracts.";
                var client = await _clientApiService.GetClientByIdAsync(id);
                return View("Delete", client);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}