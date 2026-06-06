using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    public class Favourite
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string? GameName { get; set; }
        public string? GameAction { get; set; }

        [ForeignKey("UserId")]
        public Registration? User { get; set; } 
    }
}

