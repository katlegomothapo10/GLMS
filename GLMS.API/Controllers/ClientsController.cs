using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.Models;  // This references the MVC project's Models

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] Client client)
        {
            if (id != client.ClientId) return BadRequest("ID mismatch");

            var existing = await _context.Clients.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = client.Name;
            existing.Email = client.Email;
            existing.Phone = client.Phone;
            existing.Address = client.Address;
            existing.Region = client.Region;
            existing.ContactPerson = client.ContactPerson;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

    }
}