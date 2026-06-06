using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Services;
using GLMS.Models;

namespace GLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            IApiService apiService,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _apiService = apiService;
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var serviceRequests = await _apiService.GetAsync<ServiceRequest>("api/servicerequests");
            return View(serviceRequests ?? new List<ServiceRequest>());
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var serviceRequest = await _apiService.GetByIdAsync<ServiceRequest>("api/servicerequests", id.Value);
            if (serviceRequest == null) return NotFound();

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            var contracts = await _apiService.GetAsync<Contract>("api/contracts");
            var activeContracts = contracts?.Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.Today &&
                           c.EndDate >= DateTime.Today).ToList() ?? new List<Contract>();

            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient");
            var currentRate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.CurrentExchangeRate = currentRate;

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,CostUSD,SpecialInstructions")] ServiceRequest serviceRequest)
        {
            // DEBUG: Log to console
            Console.WriteLine("=== CREATE POST HIT ===");
            Console.WriteLine($"ContractId: {serviceRequest.ContractId}");
            Console.WriteLine($"Description: {serviceRequest.Description}");
            Console.WriteLine($"CostUSD: {serviceRequest.CostUSD}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Invalid contract selected.");
            }
            else if (contract.Status != ContractStatus.Active)
            {
                ModelState.AddModelError("ContractId",
                    $"Service requests cannot be created for contracts with status '{contract.Status}'. Only Active contracts are allowed.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                    serviceRequest.CostZAR = serviceRequest.CostUSD * exchangeRate;
                    serviceRequest.ExchangeRateUsed = exchangeRate;
                    serviceRequest.RequestNumber = $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                    serviceRequest.Status = RequestStatus.Pending;
                    serviceRequest.RequestedDate = DateTime.UtcNow;

                    var created = await _apiService.PostAsync<ServiceRequest>("api/servicerequests", serviceRequest);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service request");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            var contracts = await _apiService.GetAsync<Contract>("api/contracts");
            var activeContracts = contracts?.Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.Today &&
                           c.EndDate >= DateTime.Today).ToList() ?? new List<Contract>();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);
            ViewBag.CurrentExchangeRate = await _currencyService.GetUsdToZarRateAsync();

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var serviceRequest = await _apiService.GetByIdAsync<ServiceRequest>("api/servicerequests", id.Value);
            if (serviceRequest == null) return NotFound();

            if (serviceRequest.Status != RequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending service requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var contracts = await _apiService.GetAsync<Contract>("api/contracts");
            var activeContracts = contracts?.Where(c => c.Status == ContractStatus.Active).ToList() ?? new List<Contract>();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,ContractId,Description,CostUSD,SpecialInstructions,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId) return NotFound();

            var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", serviceRequest.ContractId);

            if (contract == null || contract.Status != ContractStatus.Active)
            {
                ModelState.AddModelError("ContractId", "Selected contract is not active.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (serviceRequest.CostUSD > 0)
                    {
                        var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                        serviceRequest.CostZAR = serviceRequest.CostUSD * exchangeRate;
                        serviceRequest.ExchangeRateUsed = exchangeRate;
                    }

                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    await _apiService.PutAsync<ServiceRequest>("api/servicerequests", id, serviceRequest);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating service request");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            var contracts = await _apiService.GetAsync<Contract>("api/contracts");
            var activeContracts = contracts?.Where(c => c.Status == ContractStatus.Active).ToList() ?? new List<Contract>();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // POST: ServiceRequests/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int newStatus)
        {
            try
            {
                await _apiService.PatchAsync<object>("api/servicerequests", id, new { Status = newStatus });
                TempData["SuccessMessage"] = "Status updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}