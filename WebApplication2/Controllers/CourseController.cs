using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers
{
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using WebApplication2.AppDb;
    using WebApplication2.Models;
    using WebApplication2.ModelsDTOs;
    using WebApplication2.UdateModelsDTOs;

    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Courses.Include(c => c.Lectures).ThenInclude(l =>l.Sections).ToListAsync();
        }
        
        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = _context.Courses
                        .Include(c => c.Lectures)
                        .ThenInclude(l => l.Sections)
                        .FirstOrDefault(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return  course;
        }

        // POST: api/Course
        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse([FromForm] CourseDto dto)
        {
            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
            if (!adminExists)
            {
                return BadRequest("Admin not found.");
            }
            var courseExists = await _context.Courses
              .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

            if (courseExists)
            {
                return BadRequest("A course with the same name already exists.");
            }

            if (dto.photoLoction == null)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = Path.Combine(uploadPath, dto.photoLoction.FileName);
            try
            {
                Console.WriteLine("filePath: " + filePath);
                Console.WriteLine("PhotoLocation.FileName: " + dto.photoLoction.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.photoLoction.CopyToAsync(stream);
                }

                var course = new Course
            {
                Title = dto.Title,
                AdminId = dto.AdminId,
                photoLoction = dto.photoLoction.FileName
                
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("update/{CourseID}")]
        public async Task<IActionResult> UpdateSection(int CourseID, [FromForm] CourseUpdateDto dto)
        {
            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
            if (!adminExists)
            {
                return BadRequest("Admin not found.");
            }
      
            var course = await _context.Courses.FindAsync(CourseID);
            if (course == null)
            {
                return NotFound("Course not found.");
            }
            var courseExists = await _context.Courses
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

            if (courseExists)
            {
                return BadRequest("A course with the same name already exists.");
            }
            // تحديث البيانات الأساسية
            course.Title = dto.Title;
            course.AdminId = dto.AdminId;

            // تحديث الملف لو تم إرساله
            if (dto.photoLoction != null)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, dto.photoLoction.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.photoLoction.CopyToAsync(stream);
                }

                course.photoLoction = dto.photoLoction.FileName;
            }

            await _context.SaveChangesAsync();
            return Ok(course);
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
