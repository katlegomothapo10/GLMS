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
        public async Task<IActionResult> Index(
    string searchString,
    int? status,
    decimal? minCost,
    decimal? maxCost,
    DateTime? startDate,
    DateTime? endDate)
        {
            var requests = await _apiService.GetAsync<ServiceRequest>("api/servicerequests");

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                requests = requests?.Where(r =>
                    r.RequestNumber.Contains(searchString) ||
                    (r.Contract != null && r.Contract.ContractNumber.Contains(searchString)) ||
                    (r.Description != null && r.Description.Contains(searchString))).ToList();
            }

            if (status.HasValue)
            {
                requests = requests?.Where(r => (int)r.Status == status.Value).ToList();
            }

            if (minCost.HasValue)
            {
                requests = requests?.Where(r => r.CostUSD >= minCost.Value).ToList();
            }

            if (maxCost.HasValue)
            {
                requests = requests?.Where(r => r.CostUSD <= maxCost.Value).ToList();
            }

            if (startDate.HasValue)
            {
                requests = requests?.Where(r => r.RequestedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                requests = requests?.Where(r => r.RequestedDate <= endDate.Value).ToList();
            }

            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentMinCost = minCost;
            ViewBag.CurrentMaxCost = maxCost;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.StatusFilter = new SelectList(Enum.GetValues(typeof(RequestStatus)));

            return View(requests ?? new List<ServiceRequest>());
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
            Console.WriteLine("===== CREATE POST METHOD HIT =====");
            Console.WriteLine($"ContractId: {serviceRequest.ContractId}");
            Console.WriteLine($"Description: {serviceRequest.Description}");
            Console.WriteLine($"CostUSD: {serviceRequest.CostUSD}");

            // Remove validation for fields we handle
            ModelState.Remove("RequestNumber");
            ModelState.Remove("Status");
            ModelState.Remove("RequestedDate");
            ModelState.Remove("CostZAR");
            ModelState.Remove("ExchangeRateUsed");

            // Validate contract exists
            if (serviceRequest.ContractId <= 0)
            {
                ModelState.AddModelError("ContractId", "Please select a contract");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get contract to validate
                    var contract = await _apiService.GetByIdAsync<Contract>("api/contracts", serviceRequest.ContractId);

                    if (contract == null)
                    {
                        ModelState.AddModelError("ContractId", "Invalid contract selected");
                        return View(serviceRequest);
                    }

                    if (contract.Status != ContractStatus.Active)
                    {
                        ModelState.AddModelError("ContractId", $"Contract is {contract.Status}. Only Active contracts allowed");
                        return View(serviceRequest);
                    }

                    // Currency conversion
                    var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                    serviceRequest.CostZAR = serviceRequest.CostUSD * exchangeRate;
                    serviceRequest.ExchangeRateUsed = exchangeRate;
                    serviceRequest.RequestNumber = $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                    serviceRequest.Status = RequestStatus.Pending;
                    serviceRequest.RequestedDate = DateTime.UtcNow;

                    var created = await _apiService.PostAsync<ServiceRequest>("api/servicerequests", serviceRequest);

                    TempData["SuccessMessage"] = "Service request created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            // Repopulate dropdown
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