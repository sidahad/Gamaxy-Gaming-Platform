using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class GameRatingViewModel
    {
        [Key]
        public int Id { get; set; }
        public string? GameName { get; set; }
        public int? UserId { get; set; }
        public int ExistingRating { get; set; }
        public double AverageRating { get; set; }
    }
}