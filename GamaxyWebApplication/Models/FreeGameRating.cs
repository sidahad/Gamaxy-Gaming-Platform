using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    public class FreeGameRating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string? GameName { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime RatingDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public Registration? User { get; set; }
    }
}