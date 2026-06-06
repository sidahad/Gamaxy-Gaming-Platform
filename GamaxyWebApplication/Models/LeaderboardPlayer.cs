using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    [Table("LeaderboardPlayers")]
    public class LeaderboardPlayer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int XP { get; set; }
        public string CountryCode { get; set; } = string.Empty; // e.g., "pk"
        public int Rank { get; set; }

        public List<Feedback>? Feedbackss { get; set; }
    }
}
