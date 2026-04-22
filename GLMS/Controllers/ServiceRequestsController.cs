using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using GLMS.Services;

namespace GLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        // ServiceRequests
        public async Task<IActionResult> Index()
        {
            var serviceRequests = _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.RequestedDate);

            return View(await serviceRequests.ToListAsync());
        }

        //ServiceRequests Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(m => m.ServiceRequestId == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        //ServiceRequests Create
        public async Task<IActionResult> Create()
        {
            //only active contracts
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.Today &&
                           c.EndDate >= DateTime.Today)
                .ToListAsync();

            ViewBag.Contracts = new SelectList(activeContracts, "ContractId",
                "ContractNumberWithClient");

            //current exchange rate
            var currentRate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.CurrentExchangeRate = currentRate;

            return View();
        }

        //ServiceRequests create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,CostUSD,SpecialInstructions")] ServiceRequest serviceRequest)
        {
            // Validate contract is active before creating service request
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Invalid contract selected.");
            }
            else if (contract.Status != ContractStatus.Active)
            {
                ModelState.AddModelError("ContractId",
                    $"Service requests cannot be created for contracts with status '{contract.Status}'. Only Active contracts are allowed.");
            }
            else if (contract.StartDate > DateTime.Today || contract.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("ContractId",
                    "Contract date range is not valid. Contract must be within its valid date period.");
            }

            if (ModelState.IsValid)
            {
                //currency conversion
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                serviceRequest.CostZAR = serviceRequest.CostUSD * exchangeRate;
                serviceRequest.ExchangeRateUsed = exchangeRate;

                //unique request number
                serviceRequest.RequestNumber = $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                serviceRequest.Status = RequestStatus.Pending;
                serviceRequest.RequestedDate = DateTime.UtcNow;

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();

                //log entry
                var log = new ServiceRequestLog
                {
                    ServiceRequestId = serviceRequest.ServiceRequestId,
                    Action = "Created",
                    Details = $"Service request created for contract {contract?.ContractNumber}. USD {serviceRequest.CostUSD:F2} = ZAR {serviceRequest.CostZAR:F2} (Rate: {exchangeRate:F4})",
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = User.Identity?.Name ?? "System"
                };
                _context.ServiceRequestLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service request created: {RequestNumber} for contract {ContractNumber}",
                    serviceRequest.RequestNumber, contract?.ContractNumber);

                return RedirectToAction(nameof(Index));
            }

            //view data if validation fails
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.Today &&
                           c.EndDate >= DateTime.Today)
                .ToListAsync();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);
            ViewBag.CurrentExchangeRate = await _currencyService.GetUsdToZarRateAsync();

            return View(serviceRequest);
        }

        //ServiceRequests Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            // Only allow editing of pending requests
            if (serviceRequest.Status != RequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending service requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var activeContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // Post : ServiceRequests edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceRequestId,ContractId,Description,CostUSD,SpecialInstructions,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            var existingRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (existingRequest == null)
            {
                return NotFound();
            }

            // Validate contract still active
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == serviceRequest.ContractId);

            if (contract == null || contract.Status != ContractStatus.Active)
            {
                ModelState.AddModelError("ContractId", "Selected contract is not active.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update fields
                    existingRequest.ContractId = serviceRequest.ContractId;
                    existingRequest.Description = serviceRequest.Description;
                    existingRequest.CostUSD = serviceRequest.CostUSD;
                    existingRequest.SpecialInstructions = serviceRequest.SpecialInstructions;
                    existingRequest.Status = serviceRequest.Status;

                    // Recalculate ZAR amount if cost changed
                    if (existingRequest.CostUSD != serviceRequest.CostUSD)
                    {
                        var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                        existingRequest.CostZAR = serviceRequest.CostUSD * exchangeRate;
                        existingRequest.ExchangeRateUsed = exchangeRate;
                    }

                    existingRequest.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();

                    //log entry
                    var log = new ServiceRequestLog
                    {
                        ServiceRequestId = existingRequest.ServiceRequestId,
                        Action = "Updated",
                        Details = $"Service request updated. New status: {existingRequest.Status}",
                        Timestamp = DateTime.UtcNow,
                        PerformedBy = User.Identity?.Name ?? "System"
                    };
                    _context.ServiceRequestLogs.Add(log);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Service request updated: {RequestNumber}", existingRequest.RequestNumber);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.ServiceRequestId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var activeContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();
            ViewBag.Contracts = new SelectList(activeContracts, "ContractId", "ContractNumberWithClient", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // Post : ServiceRequests Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus newStatus)
        {
            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var oldStatus = serviceRequest.Status;
            serviceRequest.Status = newStatus;

            if (newStatus == RequestStatus.Completed)
            {
                serviceRequest.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            //log entry
            var log = new ServiceRequestLog
            {
                ServiceRequestId = serviceRequest.ServiceRequestId,
                Action = "StatusChanged",
                Details = $"Status changed from {oldStatus} to {newStatus}",
                Timestamp = DateTime.UtcNow,
                PerformedBy = User.Identity?.Name ?? "System"
            };
            _context.ServiceRequestLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Service request status updated to {newStatus}";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.ServiceRequestId == id);
        }
    }
}