using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.Models;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(ApplicationDbContext context, ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/servicerequests
        [HttpGet]
        public async Task<IActionResult> GetServiceRequests()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .OrderByDescending(s => s.RequestedDate)
                .ToListAsync();
            return Ok(requests);
        }

        // GET: api/servicerequests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceRequest(int id)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (request == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            return Ok(request);
        }

        // POST: api/servicerequests
        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] ServiceRequest request)
        {
            try
            {
                // Validate contract exists and is active
                var contract = await _context.Contracts.FindAsync(request.ContractId);
                if (contract == null)
                {
                    return BadRequest(new { message = $"Contract with ID {request.ContractId} does not exist" });
                }

                if (contract.Status != ContractStatus.Active)
                {
                    return BadRequest(new { message = $"Service requests can only be created for Active contracts. Current status: {contract.Status}" });
                }

                // Generate request number if not provided
                if (string.IsNullOrEmpty(request.RequestNumber))
                {
                    request.RequestNumber = $"SR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
                }

                request.Status = RequestStatus.Pending;
                request.RequestedDate = DateTime.UtcNow;

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                // Load contract data for response
                await _context.Entry(request).Reference(r => r.Contract).LoadAsync();

                _logger.LogInformation("Service request created: {RequestNumber}", request.RequestNumber);

                return CreatedAtAction(nameof(GetServiceRequest), new { id = request.ServiceRequestId }, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: api/servicerequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRequest(int id, [FromBody] ServiceRequest request)
        {
            if (id != request.ServiceRequestId) return BadRequest("ID mismatch");

            var existing = await _context.ServiceRequests.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ContractId = request.ContractId;
            existing.Description = request.Description;
            existing.CostUSD = request.CostUSD;
            existing.CostZAR = request.CostZAR;
            existing.Status = request.Status;
            existing.ExchangeRateUsed = request.ExchangeRateUsed;
            existing.SpecialInstructions = request.SpecialInstructions;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // PATCH: api/servicerequests/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateServiceRequestStatus(int id, [FromBody] int status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            var oldStatus = request.Status;
            request.Status = (RequestStatus)status;

            if (request.Status == RequestStatus.Completed)
            {
                request.CompletedDate = DateTime.UtcNow;
            }

            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service request {RequestNumber} status changed from {OldStatus} to {NewStatus}",
                request.RequestNumber, oldStatus, request.Status);

            return Ok(new { message = "Status updated successfully", status = request.Status.ToString() });
        }

        // DELETE: api/servicerequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}