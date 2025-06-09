using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using WebApplication2.AppDb;
using WebApplication2.AuthServices;
using WebApplication2.Models;
using WebApplication2.ModelsDTOs;
using WebApplication2.UdateModelsDTOs;
namespace WebApplication2.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    public class SectionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;


        public SectionController(AppDbContext context,FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Section>>> GetSections()
        {
            return await _context.Sections.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SectionReadDto>> GetSection(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return NotFound();
            var dto = new SectionReadDto
            {
                SectionId = section.SectionId,
                Title = section.Title,
                LectureId = section.LectureId,
                AdminId = section.AdminId,
                SectionPDF = section.SectionPDF

            };
            return Ok(dto);
        }
     [HttpPost]
        public async Task<IActionResult> CreateSection([FromForm] SectionCreateDto dto)
        {
            var sectionExists = await _context.Sections
            .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower());

            if (sectionExists)
            {
                return BadRequest("A Section with the same name already exists.");
            }
            var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
            if (!adminExists)
            {
                return BadRequest("Admin not found.");
            }
     
            var lectureExists = await _context.Lectures.AnyAsync(l => l.LectureId == dto.LectureId);
            if (!lectureExists)
            {
                return BadRequest("lecture not found.");
            }

            if (dto == null)
            {
                return BadRequest("The input data is null.");
            }
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Section");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = Path.Combine(uploadPath, dto.SectionPDF.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.SectionPDF.CopyToAsync(stream);
                }
                var section = new Section
                {
                    Title = dto.Title,
                    SectionPDF = dto.SectionPDF.FileName,
                    LectureId = dto.LectureId,
                    AdminId = dto.AdminId
                };
                _context.Sections.Add(section);
                await _context.SaveChangesAsync();

                return Ok(section);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update/{SectionID}")]
        public async Task<IActionResult> UpdateSection(int SectionID, [FromForm] SectionUpdateDto dto)
        {
            var section = await _context.Sections.FindAsync(SectionID);
            if (section == null)
            {
                return NotFound("Section not found.");
            }
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var sectionExists = await _context.Sections
                    .AnyAsync(c => c.Title.ToLower() == dto.Title.ToLower() && c.SectionId != SectionID);

                if (sectionExists)
                {
                    return BadRequest("A Section with the same name already exists.");
                }

                section.Title = dto.Title;
            }

            if (dto.AdminId.HasValue)
            {
                var adminExists = await _context.Admins.AnyAsync(a => a.AdminId == dto.AdminId);
                if (!adminExists)
                {
                    return BadRequest("Admin not found.");
                }
                section.AdminId = dto.AdminId.Value;
            }

            if (dto.LectureId.HasValue)
            {
                var lectureExists = await _context.Lectures.AnyAsync(c => c.LectureId == dto.LectureId);
                if (!lectureExists)
                {
                    return BadRequest("Lecture not found.");
                }
                section.LectureId = dto.LectureId.Value;
            }

            // تحديث الملف لو تم إرساله
            if (dto.SectionPDF != null)
            {
                // اسم الملف القديم
                var oldFileName = section.SectionPDF;

                // ✅ تأكد إن الملف القديم مش مستخدم في Section تانية
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    var isFileUsedElsewhere = await _context.Sections
                        .AnyAsync(s => s.SectionId != section.SectionId && s.SectionPDF == oldFileName);

                    if (!isFileUsedElsewhere)
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Section", oldFileName);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                }

                // ✅ إنشاء مسار التخزين
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Section");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // ✅ اسم فريد للملف الجديد
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.SectionPDF.FileName);
                var newPath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await dto.SectionPDF.CopyToAsync(stream);
                }

                section.SectionPDF = uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return Ok(section);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return NotFound(new { message = "Section not found." });


            if (!string.IsNullOrEmpty(section.SectionPDF))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads\\Course\\Section",
                                            section.SectionPDF);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
            try
            {
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
                return Content("The section was deleted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
    }

}
