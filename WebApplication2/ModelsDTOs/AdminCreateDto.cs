using System.ComponentModel.DataAnnotations;

namespace WebApplication2.ModelsDTOs
{
    public class AdminCreateDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "EmailAddress Required")]
        [EmailAddress(ErrorMessage = "Enter Correct Email")]
        public string Email { get; set; } 
    }
}
