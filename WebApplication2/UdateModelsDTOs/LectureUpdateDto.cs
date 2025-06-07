namespace WebApplication2.UdateModelsDTOs
{
    public class LectureUpdateDto
    {
        public string Title { get; set; }
        public IFormFile? LectureLocation { get; set; }  // Optional
        public int CourseId { get; set; }
        public int AdminId { get; set; }

    }
}
