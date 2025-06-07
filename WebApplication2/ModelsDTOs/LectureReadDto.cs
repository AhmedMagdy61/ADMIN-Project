namespace WebApplication2.ModelsDTOs
{
    public class LectureReadDto
    {
        public int LectureId { get; set; }
        public string Title { get; set; }
        public string LectureLoction { get; set; }
        public int CourseId { get; set; }
        public int AdminId { get; set; }

        public CourseReadDto Course { get; set; }  
    }

}
