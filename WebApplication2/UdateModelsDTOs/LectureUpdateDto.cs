namespace WebApplication2.UdateModelsDTOs
{
    public class LectureUpdateDto
    {
        public string? Title { get; set; }
        public IFormFile? LecturePDF { get; set; }  // Optional
        public int? CourseId { get; set; }
        public int? AdminId { get; set; }

    }
}
