using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.AppDb;
using WebApplication2.AuthServices;
using WebApplication2.Models;
using WebApplication2.ModelsDTOs;
using WebApplication2.UdateModelsDTOs;
using static System.Collections.Specialized.BitVector32;

namespace WebApplication2.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class LectureController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;


        public LectureController(AppDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lecture>>> GetLectures()
        {
            //.Include(c => c.Course)
            var lectures = await _context.Lectures.Include(l => l.Sections)
                .Include(l => l.Course)
                .Select(l => new LectureReadDto
                {
                    LectureId = l.LectureId,
                    Title = l.Title,
                    LecturePDF = l.LecturePDF,
                    CourseId = l.CourseId,
                    TitleCourse = l.Course.Title,
                    AdminId = l.AdminId
                    

                })
        .ToListAsync();
        
            return Ok(lectures);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<LectureReadDto>> GetLecture(int id)
        {
            var lecture = await _context.Lectures
                            .Include(l => l.Course)
                            .FirstOrDefaultAsync(l => l.LectureId == id);
            //var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }
            var dto = new LectureReadDto
            {
                LectureId = lecture.LectureId,
                Title = lecture.Title,
                LecturePDF = lecture.LecturePDF,
                CourseId = lecture.CourseId,
                AdminId = lecture.AdminId,
                TitleCourse = lecture.Course.Title

            };
            
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLecture([FromForm] LectureCreateDto dto)
        {
            var lectureExists = await _context.Lectures
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

            if (lectureExists)
            {
                return BadRequest("A Lecture with the same name already exists.");
            }

            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
            if (!adminExists)
            {
                return BadRequest("Admin not found.");
            }
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
            if (!courseExists)
            {
                return BadRequest("Course not found.");
            }

            if (dto.LecturePDF == null)
            {
                return BadRequest("No file uploaded.");
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = Path.Combine(uploadPath, dto.LecturePDF.FileName);
            try
            {
                Console.WriteLine("filePath: " + filePath);
                Console.WriteLine("LectureLocation.FileName: " + dto.LecturePDF.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.LecturePDF.CopyToAsync(stream);
                }
                var lecture = new Lecture
                {
                    Title = dto.Title,
                    LecturePDF = dto.LecturePDF.FileName, // full physical path
                    CourseId = dto.CourseId,
                    AdminId = dto.AdminId
                };
                _context.Lectures.Add(lecture);
                await _context.SaveChangesAsync();

                return Ok(lecture);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update/{LectureId}")]
        public async Task<IActionResult> UpdateLecture(int LectureId, [FromForm] LectureUpdateDto dto)
        {
            var lecture = await _context.Lectures.FindAsync(LectureId);
            if (lecture == null)
            {
                return NotFound("Lecture not found.");
            }
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var lectureExists = await _context.Lectures
                    .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower() && c.LectureId != LectureId);
                if (lectureExists)
                {
                    return BadRequest("A Lecture with the same name already exists.");
                }

                lecture.Title = dto.Title;
            }
   
            if (dto.AdminId.HasValue)
            {
                var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
                if (!adminExists)
                {
                    return BadRequest("Admin not found.");
                }
                lecture.AdminId = dto.AdminId.Value;
            }
            if (dto.CourseId.HasValue)
            {
                var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId);
                if (!courseExists)
                {
                    return BadRequest("Course not found.");
                }
                lecture.CourseId = dto.CourseId.Value;
            }
           
            // تحديث الملف لو تم إرساله
            if (dto.LecturePDF != null)
            {
                var oldFileName = lecture.LecturePDF;

                if (!string.IsNullOrEmpty(oldFileName))
                {
                    var isFileUsedElsewhere = await _context.Lectures
                    .AnyAsync(s => s.LectureId != lecture.LectureId && s.LecturePDF == oldFileName);
                    if (!isFileUsedElsewhere)
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture", oldFileName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                }
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.LecturePDF.FileName);
                var newPath = Path.Combine(uploadPath, uniqueFileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await dto.LecturePDF.CopyToAsync(stream);
                }

                lecture.LecturePDF = uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return Ok(lecture);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
                return NotFound(new { message = "Lecture not found." });

            if (!string.IsNullOrEmpty(lecture.LecturePDF))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture", lecture.LecturePDF);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
            try
            {
                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();
                return Content("The lecture was deleted with its section.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
    }

}
