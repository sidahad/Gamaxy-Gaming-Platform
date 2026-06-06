using System.ComponentModel.DataAnnotations;

namespace GamaxyWebApplication.Models
{
    public class HomePageSectionsViewModel
    {
        [Key]
        public int id { get; set; }
        public List<string>? TrendingGames { get; set; }
        public List<string>? FeaturedGames { get; set; }
        public List<string>? Games { get; set; }
    }

}
