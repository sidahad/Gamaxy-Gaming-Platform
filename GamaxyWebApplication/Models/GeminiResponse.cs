using GamaxyWebApplication;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GamaxyWebApplication.Models
{
    public class GeminiResponse
    {
        public string Prompt { get; set; }
        public string Text { get; set; }
        public string SessionId { get; set; }

        public List<ChatMessage> ChatHistory { get; set; } = new();
        public List<IFormFile> Files { get; set; } = new();

        public List<UploadedFile> UploadedFiles { get; set; } = new();
    }
}
