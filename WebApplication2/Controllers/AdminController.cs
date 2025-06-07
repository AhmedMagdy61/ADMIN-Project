using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.AppDb;
using WebApplication2.Models;
using WebApplication2.ModelsDTOs;

namespace WebApplication2.Controllers
{
  

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdmins()
        {
            return await _context.Admins.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminReadDto>> GetAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound();
            var dto = new AdminReadDto
            {
                AdminId = admin.AdminId,
                Username = admin.Username,
                Email = admin.Email
                
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<Admin>> CreateAdmin([FromForm] AdminCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email Required");
            var hasher = new PasswordHasher<object>();

            var admin = new Admin
            {
                Username = dto.Username,
                PasswordHash = hasher.HashPassword(null, dto.PasswordHash),
                Email = dto.Email

            };
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return Ok(admin);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound();

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
