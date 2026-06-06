using GamaxyWebApplication.Models;
namespace GamaxyWebApplication
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public string? SessionId { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public byte[] Data { get; set; }
    }
}