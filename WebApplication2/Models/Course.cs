using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Photo { get; set; } = string.Empty;
        public int AdminId { get; set; }
        // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonIgnore]
        public Admin Admin { get; set; }

        public ICollection<Lecture> Lectures { get; set; }
    }

}
