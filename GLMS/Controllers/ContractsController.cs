using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace GLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ContractsController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // Contracts
        public async Task<IActionResult> Index(
            string searchString,
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status)
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            // filters using LINQ
            if (!string.IsNullOrEmpty(searchString))
            {
                contracts = contracts.Where(c =>
                    c.ContractNumber.Contains(searchString) ||
                    (c.Client != null && c.Client.Name.Contains(searchString)));
            }

            if (startDate.HasValue)
            {
                contracts = contracts.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                contracts = contracts.Where(c => c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                contracts = contracts.Where(c => c.Status == status.Value);
            }

            // Update statuses (dates)
            var allContracts = await contracts.ToListAsync();
            foreach (var contract in allContracts)
            {
                if (contract.Status == ContractStatus.Active && contract.EndDate < DateTime.Today)
                {
                    contract.Status = ContractStatus.Expired;
                    _context.Entry(contract).State = EntityState.Modified;
                }
            }
            await _context.SaveChangesAsync();

            ViewBag.StatusFilter = new SelectList(Enum.GetValues(typeof(ContractStatus)));
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentSearchString = searchString;

            return View(allContracts);
        }

        // Contracts Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create - This displays the form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "Name");
            return View();
        }

        // POST: Contracts/Create - This saves the data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel,ContractValueUSD,SpecialTerms")] Contract contract, IFormFile? SignedAgreement)
        {
            if (ModelState.IsValid)
            {
                // Generate contract number
                contract.ContractNumber = $"CT-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                contract.CreatedAt = DateTime.UtcNow;

                // Handle file upload
                if (SignedAgreement != null && SignedAgreement.Length > 0)
                {
                    var extension = Path.GetExtension(SignedAgreement.FileName).ToLower();
                    if (extension == ".pdf")
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contracts");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{contract.ContractNumber}_{DateTime.Now:yyyyMMddHHmmss}_{SignedAgreement.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await SignedAgreement.CopyToAsync(stream);
                        }

                        contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
                    }
                }

                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // Contracts edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        // Post : Contracts edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContractId,ContractNumber,ClientId,StartDate,EndDate,Status,ServiceLevel,ContractValueUSD,SpecialTerms,SignedAgreementPath")] Models.Contract contract, IFormFile? SignedAgreement)
        {
            if (id != contract.ContractId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //new file upload
                    if (SignedAgreement != null && SignedAgreement.Length > 0)
                    {
                        var extension = Path.GetExtension(SignedAgreement.FileName).ToLower();
                        if (extension != ".pdf")
                        {
                            ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "Name", contract.ClientId);
                            return View(contract);
                        }

                        // Delete old file
                        if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contracts");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = $"{contract.ContractNumber}_{DateTime.Now:yyyyMMddHHmmss}_{SignedAgreement.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await SignedAgreement.CopyToAsync(fileStream);
                        }

                        contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
                    }

                    contract.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(contract).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Contract updated: {ContractNumber}", contract.ContractNumber);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.ContractId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Clients = new SelectList(await _context.Clients.ToListAsync(), "ClientId", "Name", contract.ClientId);
            return View(contract);
        }

        //Contracts Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.ContractId == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // Post : Contracts delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                // Delete file
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Contract deleted: {ContractNumber}", contract.ContractNumber);
            }

            return RedirectToAction(nameof(Index));
        }

        //Contracts download file
        public async Task<IActionResult> DownloadFile(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
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

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ContractId == id);
        }
    }
}