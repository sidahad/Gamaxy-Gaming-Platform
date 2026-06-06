using GamaxyWebApplication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GamaxyWebApplication.Models;

namespace GamaxyWebApplication.Models
{
    public class MatchAssign
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public int MatchId { get; set; }
        public Match? Match { get; set; }

        [NotMapped]
        public List<SelectListItem>? UserList { get; set; }
        [NotMapped]
        public List<SelectListItem>? MatchList { get; set; }
        public List<User>? AssignedUser { get; set; }
        public List<User>? UnassignedUser { get; set; }
    }
}
