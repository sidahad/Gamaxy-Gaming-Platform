using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class ResetPasswordViewModel
    {
        [Key]
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }

}
