namespace GamaxyWebApplication.Models
{
    public class EmailSettings
    {
        public string? FromEmail { get; set; }
        public string? AppPassword { get; set; }
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
    }
}
