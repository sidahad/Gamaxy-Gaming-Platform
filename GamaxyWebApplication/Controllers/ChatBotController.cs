using GamaxyWebApplication.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using GamaxyWebApplication.Data;

namespace GamaxyWebApplication.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _db;
        private const string ApiKey = "AIzaSyBsan1woFCACKk5MLEsgikbgO5OihbBPC4";

        public ChatBotController(IHttpClientFactory httpClientFactory, ApplicationDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string sessionId = null)
        {
            sessionId ??= Guid.NewGuid().ToString();

            var model = new GeminiResponse
            {
                SessionId = sessionId,
                ChatHistory = _db.ChatMessages
                    .Where(c => c.SessionId == sessionId)
                    .OrderBy(c => c.Timestamp)
                    .ToList()
            };

            ViewBag.AllSessions = _db.ChatMessages
                .Select(c => c.SessionId)
                .Distinct()
                .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Generate(GeminiResponse model, List<IFormFile> UploadedFiles)
        {
            if (string.IsNullOrWhiteSpace(model.Prompt) && (UploadedFiles == null || UploadedFiles.Count == 0))
                return RedirectToAction("Index");

            var requestParts = new List<object>();

            // Add previous chat history to the request
            var previousMessages = _db.ChatMessages
                .Where(c => c.SessionId == model.SessionId)
                .OrderBy(c => c.Timestamp)
                .ToList();

            foreach (var message in previousMessages)
            {
                requestParts.Add(new { text = message.Message });
            }

            // Save user prompt
            if (!string.IsNullOrWhiteSpace(model.Prompt))
            {
                _db.ChatMessages.Add(new ChatMessage
                {
                    SessionId = model.SessionId,
                    Sender = "User",
                    Message = model.Prompt
                });
            }

            // Save uploaded files and add images to Gemini request
            if (UploadedFiles != null && UploadedFiles.Count > 0)
            {
                foreach (var file in UploadedFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                        var fileType = file.ContentType;

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        byte[] fileBytes;
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }

                        string base64Data = Convert.ToBase64String(fileBytes);

                        _db.ChatFiles.Add(new ChatFile
                        {
                            SessionId = model.SessionId,
                            FileName = fileName,
                            FileType = fileType,
                            FileSize = file.Length,
                            Base64Data = base64Data
                        });

                        string fileDisplay = fileType.StartsWith("image/")
                            ? $"🖼️ Uploaded image:<br><img src='/uploads/{fileName}' class='img-fluid rounded' />"
                            : $"📎 Uploaded file: <a href='/uploads/{fileName}' target='_blank'>{fileName}</a>";

                        _db.ChatMessages.Add(new ChatMessage
                        {
                            SessionId = model.SessionId,
                            Sender = "User",
                            Message = fileDisplay
                        });

                        // 👇 Only add image file types to Gemini request
                        if (fileType.StartsWith("image/"))
                        {
                            requestParts.Add(new
                            {
                                inlineData = new
                                {
                                    mimeType = fileType,
                                    data = base64Data
                                }
                            });
                        }
                    }
                }
            }

            // Add prompt text to Gemini request
            if (!string.IsNullOrWhiteSpace(model.Prompt))
            {
                requestParts.Add(new { text = model.Prompt });
            }

            await _db.SaveChangesAsync();

            // 🔥 Call Gemini API if we have something to send
            if (requestParts.Count > 0)
            {
                var client = _httpClientFactory.CreateClient();
                var requestBody = new
                {
                    contents = new[] { new { parts = requestParts } }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={ApiKey}",
                    content);

                var json = await response.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<GeminiApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var botText = string.Join("", apiResp.Candidates.SelectMany(c => c.Content.Parts).Select(p => p.Text));

                _db.ChatMessages.Add(new ChatMessage
                {
                    SessionId = model.SessionId,
                    Sender = "Bot",
                    Message = botText
                });

                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { sessionId = model.SessionId });
        }





        [HttpPost]
        public IActionResult DeleteSession(string sessionId)
        {
            var messages = _db.ChatMessages.Where(c => c.SessionId == sessionId).ToList();
            _db.ChatMessages.RemoveRange(messages);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
