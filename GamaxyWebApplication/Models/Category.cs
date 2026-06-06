using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string? CategoryName { get; set; }
        public string? IconClass { get; set; } 
        public ICollection<Game>? games { get; set; }
    }
}
