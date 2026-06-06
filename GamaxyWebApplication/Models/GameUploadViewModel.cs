using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamaxyWebApplication.Models
{
    public class GameUploadViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public int? CatId { get; set; }

        [ForeignKey("CatId")]
        public Category? Catcode { get; set; }

        [Required]
        public string? ShortDescription { get; set; }

        [Required]
        public string? LongDescription { get; set; }

        public decimal? Price { get; set; }

        [Required]
        public IFormFile? GameFile { get; set; }
        public string? UploadMethod { get; set; } 
        public string? HowToPlay { get; set; }

        [Required]
        public IFormFile? ThumbnailImage { get; set; }

        public List<Game>? Games { get; set; }
        public List<Category>? Categories { get; set; }
        }

    }
