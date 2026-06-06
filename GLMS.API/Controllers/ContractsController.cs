using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.Models;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetContracts()
        {
            var contracts = await _context.Contracts.ToListAsync();
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] Contract contract)
        {
            if (contract.ClientId <= 0)
                return BadRequest("ClientId is required");

            // Generate contract number
            contract.ContractNumber = $"CT-{DateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            contract.CreatedAt = DateTime.UtcNow;

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContract), new { id = contract.ContractId }, contract);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] Contract contract)
        {
            if (id != contract.ContractId) return BadRequest("ID mismatch");

            var existing = await _context.Contracts.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.Status = contract.Status;
            existing.ServiceLevel = contract.ServiceLevel;
            existing.ContractValueUSD = contract.ContractValueUSD;
            existing.SpecialTerms = contract.SpecialTerms;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
    }
}