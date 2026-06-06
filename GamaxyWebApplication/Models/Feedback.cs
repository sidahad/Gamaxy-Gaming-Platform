namespace GamaxyWebApplication.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public DateTime DateSubmitted { get; set; } = DateTime.Now;
    }
}
