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
    using static System.Collections.Specialized.BitVector32;

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

            if (dto.Photo == null)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = Path.Combine(uploadPath, dto.Photo.FileName);
            try
            {
                Console.WriteLine("filePath: " + filePath);
                Console.WriteLine("PhotoLocation.FileName: " + dto.Photo.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                var course = new Course
                {
                    Title = dto.Title,
                    AdminId = dto.AdminId,
                    Photo = dto.Photo.FileName,
                    Lectures = new List<Lecture>()
                };
                
                _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            var courseWithLectures = await _context.Courses
                    .Include(c => c.Lectures)
                    .FirstOrDefaultAsync(c => c.CourseId == course.CourseId);


                return Ok(courseWithLectures);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("update/{CourseID}")]
        public async Task<IActionResult> UpdateSection(int CourseID, [FromForm] CourseUpdateDto dto)
        {
            var course = await _context.Courses.FindAsync(CourseID);
           
            if (course == null)
            {
                return NotFound("Course not found.");
            }
            if (dto.AdminId.HasValue)
            {
                var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
                if (!adminExists)
                {
                    return BadRequest("Admin not found.");
                }
                course.AdminId = dto.AdminId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var courseExists = await _context.Courses
                    .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower() && c.CourseId != CourseID);

                if (courseExists)
                {
                    return BadRequest("A course with the same name already exists.");
                }

                course.Title = dto.Title;
            }
          
            if (dto.Photo != null)
            {
                var oldFileName = course.Photo;
                if (!string.IsNullOrEmpty(course.Photo))
                {
                    var isFileUsedElsewhere = await _context.Courses
                                      .AnyAsync(s => s.CourseId != course.CourseId && s.Photo == oldFileName);
                    if (!isFileUsedElsewhere)
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course", oldFileName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Photo.FileName);
                var newPath = Path.Combine(uploadPath, uniqueFileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                course.Photo = uniqueFileName;
            }

            await _context.SaveChangesAsync();
            var courseWithLectures = await _context.Courses
                  .Include(c => c.Lectures).ThenInclude(s=> s.Sections)
                  .FirstOrDefaultAsync(c => c.CourseId == CourseID);
            return Ok(courseWithLectures);
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found." });
            if (!string.IsNullOrEmpty(course.Photo))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course", course.Photo);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return Content("The Course was deleted with its Lectures and Sections.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}
