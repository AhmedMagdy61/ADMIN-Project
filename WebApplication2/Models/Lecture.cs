using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace WebApplication2.Models
{
    public class Lecture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LectureId { get; set; }
        public string Title { get; set; }

        public string LectureLocation { get; set; }
        public int CourseId { get; set; }


        public int AdminId { get; set; }
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonIgnore]
        public Admin Admin { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Course Course { get; set; }
        public ICollection<Section>? Sections { get; set; }
    }

}
