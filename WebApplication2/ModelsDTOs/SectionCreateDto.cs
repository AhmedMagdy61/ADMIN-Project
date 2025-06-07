namespace WebApplication2.ModelsDTOs
{
    public class SectionCreateDto
    {
        public string Title { get; set; }
        public IFormFile SectionLocation { get; set; }  // يمكن التعامل مع الملف هنا بشكل خاص عند رفعه
        public int LectureId { get; set; }
        public int AdminId { get; set; }
    }

}
