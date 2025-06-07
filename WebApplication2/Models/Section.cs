using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class Section
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SectionId { get; set; }
        public string Title { get; set; }

        public string SectionLocation { get; set; }

        public int LectureId { get; set; }
       

        public int AdminId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Admin Admin { get; set; }
        [JsonIgnore]
        public Lecture Lecture { get; set; }
    }

}
