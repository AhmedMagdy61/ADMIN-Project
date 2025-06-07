using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.AppDb;
using WebApplication2.AuthServices;
using WebApplication2.Models;
using WebApplication2.ModelsDTOs;
using WebApplication2.UdateModelsDTOs;

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
            var lectures = await _context.Lectures.Include(s => s.Sections)
                .Include(l => l.Course)
                .Select(l => new LectureReadDto
                {
                    LectureId = l.LectureId,
                    Title = l.Title,
                    LectureLoction = l.LectureLocation,
                    CourseId = l.CourseId,
                    AdminId = l.AdminId,

                    Course = new CourseReadDto
                    {
                        CourseId = l.Course.CourseId,
                        Title = l.Course.Title
                        
                    },
                    
                    
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
                LectureLoction = lecture.LectureLocation,
                CourseId = lecture.CourseId,
                AdminId = lecture.AdminId,
                Course = new CourseReadDto
                {
                    CourseId = lecture.Course.CourseId,
                    Title = lecture.Course.Title

                }

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

            if (dto.LectureLocation == null)
            {
                return BadRequest("No file uploaded.");
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = Path.Combine(uploadPath, dto.LectureLocation.FileName);
            try
            {
                Console.WriteLine("filePath: " + filePath);
                Console.WriteLine("LectureLocation.FileName: " + dto.LectureLocation.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.LectureLocation.CopyToAsync(stream);
                }
                var lecture = new Lecture
                {
                    Title = dto.Title,
                    LectureLocation = dto.LectureLocation.FileName, // full physical path
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
            var lecture = await _context.Lectures.FindAsync(LectureId);
            if (lecture == null)
            {
                return NotFound("Lecture not found.");
            }

            // تحديث البيانات الأساسية
            lecture.Title = dto.Title;
            lecture.CourseId = dto.CourseId;
            lecture.AdminId = dto.AdminId;

            // تحديث الملف لو تم إرساله
            if (dto.LectureLocation != null)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Lecture");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, dto.LectureLocation.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.LectureLocation.CopyToAsync(stream);
                }

                lecture.LectureLocation = dto.LectureLocation.FileName;
            }

            await _context.SaveChangesAsync();
            return Ok(lecture);
        }
        //if (ModelState.IsValid)
        //{
        //    // حفظ الملف
        //    if (dto.LectureLocation != null)
        //    {
        //        var filePath = Path.Combine("wwwroot/uploads", dto.LectureLocation.FileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await lecture.LectureLocation.CopyToAsync(stream);
        //        }

        //        lecture.LectureFileName = filePath;
        //    }

        //    _context.Lectures.Add(lecture);
        //    await _context.SaveChangesAsync();

        //    return Ok(lecture);
        //}
        //    return BadRequest(ModelState);
        //}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
                return NotFound();

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
