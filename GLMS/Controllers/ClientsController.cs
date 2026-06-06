using Microsoft.AspNetCore.Mvc;
using GLMS.Services;
using GLMS.Models;

namespace GLMS.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IApiService _apiService;

        public ClientsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _apiService.GetAsync<Client>("api/clients");
            return View(clients ?? new List<Client>());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _apiService.GetByIdAsync<Client>("api/clients", id.Value);
            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Address,Region,ContactPerson")] Client client)
        {
            if (ModelState.IsValid)
            {
                client.CreatedAt = DateTime.UtcNow;
                var created = await _apiService.PostAsync<Client>("api/clients", client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _apiService.GetByIdAsync<Client>("api/clients", id.Value);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,Email,Phone,Address,Region,ContactPerson,CreatedAt")] Client client)
        {
            if (id != client.ClientId) return NotFound();

            if (ModelState.IsValid)
            {
                await _apiService.PutAsync<Client>("api/clients", id, client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _apiService.GetByIdAsync<Client>("api/clients", id.Value);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteAsync("api/clients", id);
            return RedirectToAction(nameof(Index));
        }
    }
}