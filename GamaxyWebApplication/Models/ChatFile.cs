using GamaxyWebApplication.Models;
namespace GamaxyWebApplication
{
    public class ChatFile
    {
        public int Id { get; set; }
        public string? SessionId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public string? Base64Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
