using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class ForgetPasswordViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? NewPassword { get; set; }
    }

}

