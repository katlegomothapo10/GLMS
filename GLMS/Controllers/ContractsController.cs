using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Services;
using GLMS.Models;

namespace GLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IApiService _apiService;

        public ContractsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(
            string searchString,
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status)
        {
            // Get contracts from API
            var contracts = await _apiService.GetAsync<Contract>("api/contracts");

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                contracts = contracts?.Where(c =>
                    c.ContractNumber.Contains(searchString) ||
                    (c.Client != null && c.Client.Name.Contains(searchString))).ToList();
            }

            if (startDate.HasValue)
            {
                contracts = contracts?.Where(c => c.StartDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                contracts = contracts?.Where(c => c.EndDate <= endDate.Value).ToList();
            }

            if (status.HasValue)
            {
                contracts = contracts?.Where(c => c.Status == status.Value).ToList();
            }

            // Update expired contracts via API
            if (contracts != null)
            {
                foreach (var contract in contracts)
                {
                    if (contract.Status == ContractStatus.Active && contract.EndDate < DateTime.Today)
                    {
                        contract.Status = ContractStatus.Expired;
                        await _apiService.PatchAsync<object>("api/contracts", contract.ContractId, new { Status = (int)ContractStatus.Expired });
                    }
                }
            }

            ViewBag.StatusFilter = new SelectList(Enum.GetValues(typeof(ContractStatus)));
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentSearchString = searchString;

            return View(contracts ?? new List<Contract>());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", id.Value);
            if (contract == null) return NotFound();

            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            var clients = await _apiService.GetAsync<Client>("api/clients");
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel,ContractValueUSD,SpecialTerms")] Contract contract)
        {
            ModelState.Remove("ContractNumber");
            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");

            if (ModelState.IsValid)
            {
                contract.ContractNumber = $"CT-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                contract.CreatedAt = DateTime.UtcNow;

                var created = await _apiService.PostAsync<Contract>("api/contracts", contract);
                return RedirectToAction(nameof(Index));
            }

            var clients = await _apiService.GetAsync<Client>("api/clients");
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", id.Value);
            if (contract == null) return NotFound();

            var clients = await _apiService.GetAsync<Client>("api/clients");
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ContractNumber,ClientId,StartDate,EndDate,Status,ServiceLevel,ContractValueUSD,SpecialTerms")] Contract contract)
        {
            if (id != contract.ContractId) return NotFound();

            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");

            if (ModelState.IsValid)
            {
                contract.UpdatedAt = DateTime.UtcNow;
                await _apiService.PutAsync<Contract>("api/contracts", id, contract);
                return RedirectToAction(nameof(Index));
            }

            var clients = await _apiService.GetAsync<Client>("api/clients");
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", id.Value);
            if (contract == null) return NotFound();

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiService.DeleteAsync("api/contracts", id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/DownloadFile/5
        public async Task<IActionResult> DownloadFile(int id)
        {
            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementPath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var fileName = Path.GetFileName(contract.SignedAgreementPath);
            return File(memory, "application/pdf", fileName);
        }
    }
}