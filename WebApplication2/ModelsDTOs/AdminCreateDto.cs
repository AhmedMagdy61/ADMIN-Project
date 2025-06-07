using System.ComponentModel.DataAnnotations;

namespace WebApplication2.ModelsDTOs
{
    public class AdminCreateDto
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        [Required(ErrorMessage = "EmailAddress Required")]
        [EmailAddress(ErrorMessage = "Enter Correct Email")]
        public string Email { get; set; } 
    }
}
