namespace WebApplication2.UdateModelsDTOs
{
    public class SectionUpdateDto
    {
        public string? Title { get; set; }
        public IFormFile? SectionLocation { get; set; }  // يمكن التعامل مع الملف هنا بشكل خاص عند رفعه
        public int? LectureId { get; set; }
        public int? AdminId { get; set; }
    }
}
