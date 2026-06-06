using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GamaxyWebApplication.Models
{

    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? UserImage { get; set; }
        public int? matchid { get; set; }
        [ForeignKey("matchid")]
        public Match? Matchcode { get; set; }

        public DateTime JoiningDate { get; set; } = DateTime.Now;
        public int? RegistrationId { get; set; }  
        [ForeignKey("RegistrationId")]
        public Registration? Registrations { get; set; }

        public virtual List<Favourite>? Favourites { get; set; }

    }
}