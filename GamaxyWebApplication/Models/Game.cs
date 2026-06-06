using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }
        public int? GamerId { get; set; }
        public Gamer? Gamer { get; set; }
        public string? Title { get; set; }
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        [Required(ErrorMessage = "Please provide 'How to Play' instructions.")]
        public string? HowToPlay { get; set; }

        public decimal? Price { get; set; }
        public string? FilePath { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? ImagePath { get; set; }
        public string? UploadSource { get; set; }
        public string? UploadedBy { get; set; }

        public string? Slug { get; set; }  
        [NotMapped]
        public double AverageRating { get; set; }

        public bool IsDeleted { get; set; } = false;
    }


}