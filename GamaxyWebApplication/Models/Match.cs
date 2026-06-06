using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class Match
    {
        [Key]
        public int MatchId { get; set; }
        public string? MatchCode { get; set; }
    }
}
