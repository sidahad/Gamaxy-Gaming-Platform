 using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? UserImage { get; set; }
        public string Role { get; set; } = "User";
        public string? GamerCode { get; set; }
        public string? VerificationCode { get; set; }
        public bool IsVerified { get; set; } = false;

    }

}
