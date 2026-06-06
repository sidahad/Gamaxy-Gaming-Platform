using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class GamerPlayinghistory
    {
        [Key]
        public int PlayHistoryId { get; set; }

        public int? PremiumGameId { get; set; }  // Sirf Premium (dynamic) ke liye

        public string? StaticGameName { get; set; } // ✅ Static (non-dynamic) ke liye

        public int? GamerId { get; set; }
        public int? UserId { get; set; }
        public DateTime PlayedOn { get; set; }

        // Navigation
        public Game? PremiumGame { get; set; }
        public Gamer? Gamer { get; set; }
        public User? User { get; set; }
        public int? Score { get; set; }
        public int? Duration { get; set; }
    }


}
