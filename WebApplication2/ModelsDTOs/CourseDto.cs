using System.ComponentModel.DataAnnotations;
using WebApplication2.Models;
using WebApplication2.UdateModelsDTOs;

namespace WebApplication2.ModelsDTOs
{
    public class CourseDto
    {
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "AdminId is required.")]
        public int AdminId { get; set; }
        [Required(ErrorMessage = "photo is required.")]
        public IFormFile Photo { get; set; }
    }
}
