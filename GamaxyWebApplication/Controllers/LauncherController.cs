using System.Diagnostics;
using System.Security.Claims;
using GamaxyWebApplication.Data;
using GamaxyWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GamaxyWebApplication.Controllers
{
    public class LauncherController : Controller
    {
        private string GenerateSlug(string title)
        {
            string slug = title.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            slug = slug.Substring(0, slug.Length <= 80 ? slug.Length : 80).Trim();
            slug = slug.Replace(" ", "-");

            var baseSlug = slug;
            int counter = 1;
            while (_dbcontext.Games.Any(g => g.Slug == slug))
            {
                slug = $"{baseSlug}-{counter++}";
            }

            return slug;
        }

        private readonly ILogger<LauncherController> _logger;
        private readonly ApplicationDbContext _dbcontext;
        IWebHostEnvironment env;

        public LauncherController(ILogger<LauncherController> logger, ApplicationDbContext db, IWebHostEnvironment env)
        {
            _logger = logger;
            this._dbcontext = db;
            this.env = env;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogInformation("UserId is null in session. Redirecting to Home/Login.");
                return RedirectToAction("Login", "Home");
            }

            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("User email is null or empty. Redirecting to Home/Login.");
                return RedirectToAction("Login", "Home");
            }

            var games = _dbcontext.Games
                .Include(g => g.Category)
                .ToList();

            var categories = _dbcontext.Categories.ToList();

            var viewModel = new GameUploadViewModel
            {
                Games = games,
                Categories = categories
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Gamer")]
        public async Task<IActionResult> LaunchingHistory()
        {
            int? gamerId = HttpContext.Session.GetInt32("UserId");
            if (gamerId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var userEmail = User.Identity?.Name;
            var uploadedGames = await _dbcontext.Games
                .Where(g => g.UploadedBy == userEmail)
                .Include(g => g.Category)
                .OrderByDescending(g => g.UploadedDate)
                .ToListAsync();

            if (!uploadedGames.Any())
            {
                ViewBag.NoGames = "You haven’t uploaded any games yet.";
            }

            return View(uploadedGames);
        }

        [HttpPost]
        public async Task<IActionResult> MoveToRecycleBin(int id)
        {
            var game = await _dbcontext.Games.FindAsync(id);
            if (game == null)
                return NotFound();

            game.IsDeleted = true;
            await _dbcontext.SaveChangesAsync();

            return RedirectToAction("LaunchingHistory");
        }

        public async Task<IActionResult> RecycleBin()
        {
            var deletedGames = await _dbcontext.Games
                .Where(g => g.IsDeleted)
                .Include(g => g.Category)
                .OrderByDescending(g => g.UploadedDate)
                .ToListAsync();

            return View(deletedGames);
        }

        [HttpPost]
        public async Task<IActionResult> Recover(int id)
        {
            var game = await _dbcontext.Games.FindAsync(id);
            if (game == null)
                return NotFound();

            game.IsDeleted = false;
            await _dbcontext.SaveChangesAsync();

            return RedirectToAction("RecycleBin");
        }

        [HttpPost]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var game = await _dbcontext.Games.FindAsync(id);
            if (game == null)
                return NotFound();

            if (!string.IsNullOrEmpty(game.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", game.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            if (!string.IsNullOrEmpty(game.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", game.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _dbcontext.Games.Remove(game);
            await _dbcontext.SaveChangesAsync();

            return RedirectToAction("RecycleBin");
        }

        [Authorize(Roles = "Gamer")]
        public IActionResult MyGames()
        {
            var userEmail = User.Identity?.Name;

            var viewModel = new GameUploadViewModel
            {
                Games = _dbcontext.Games
                   .Where(g => g.UploadedBy == userEmail && !g.IsDeleted)
                   .Include(g => g.Category)
                   .ToList(),
                Categories = _dbcontext.Categories.ToList()
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public IActionResult RateGame(int gameId, int stars)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "Unable to identify user." });
            }

            int userId = int.Parse(userIdClaim);
            if (stars < 1 || stars > 5)
            {
                return Json(new { success = false, message = "Rating must be between 1 and 5." });
            }

            var game = _dbcontext.Games.FirstOrDefault(g => g.GameId == gameId);
            if (game == null)
            {
                return Json(new { success = false, message = "Game not found." });
            }

            var existing = _dbcontext.PremiumGameRatings
                .FirstOrDefault(r => r.GameId == gameId && r.UserId == userId);

            if (existing != null)
            {
                existing.Rating = stars;
                existing.RatingDate = DateTime.Now;
                _dbcontext.Update(existing);
            }
            else
            {
                _dbcontext.PremiumGameRatings.Add(new PremiumGameRating
                {
                    UserId = userId,
                    GameId = gameId,
                    GameName = game.Title,
                    Rating = stars,
                    RatingDate = DateTime.Now
                });
            }

            _dbcontext.SaveChanges();

            return Json(new { success = true, message = "Rating submitted successfully!" });
        }

        [Authorize]
        public IActionResult MyPremiumGameReviewsandRatings()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = int.Parse(userIdClaim);
            var ratings = _dbcontext.PremiumGameRatings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RatingDate)
                .ToList();

            return View(ratings);
        }

        [Authorize(Roles = "Gamer")]
        [HttpGet]
        public IActionResult UploadGame()
        {
            ViewBag.CategoriesSl = new SelectList(_dbcontext.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [Authorize(Roles = "Gamer")]
        [HttpPost]
        public async Task<IActionResult> UploadGame(GameUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesSl = new SelectList(_dbcontext.Categories, "CategoryId", "CategoryName");
                return View(model);
            }

            string gameFileName = Guid.NewGuid() + Path.GetExtension(model.GameFile.FileName);
            string imageFileName = Guid.NewGuid() + Path.GetExtension(model.ThumbnailImage.FileName);

            var gameFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploadedGames");
            var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "gameThumbnails");

            Directory.CreateDirectory(gameFolder);
            Directory.CreateDirectory(imageFolder);

            var gamePath = Path.Combine(gameFolder, gameFileName);
            using (var stream = new FileStream(gamePath, FileMode.Create))
            {
                await model.GameFile.CopyToAsync(stream);
            }

            var imagePath = Path.Combine(imageFolder, imageFileName);
            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await model.ThumbnailImage.CopyToAsync(stream);
            }

            var game = new Game
            {
                Title = model.Title,
                ShortDescription = model.ShortDescription,
                LongDescription = model.LongDescription,
                Price = model.Price,
                FilePath = "/uploadedGames/" + gameFileName,
                ImagePath = "/gameThumbnails/" + imageFileName,
                UploadedDate = DateTime.Now,
                CategoryId = model.CatId,
                UploadSource = model.UploadMethod,
                UploadedBy = User.Identity?.Name,
                Slug = GenerateSlug(model.Title),
                HowToPlay = model.HowToPlay
            };

            _dbcontext.Games.Add(game);
            await _dbcontext.SaveChangesAsync();

            return Json(new { gameId = game.GameId });
        }

        [HttpGet("/Game/{slug}")]
        public IActionResult Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var game = _dbcontext.Games
                .Include(g => g.Category)
                .FirstOrDefault(g => g.Slug == slug);

            if (game == null)
                return NotFound();

            return View(game);
        }

        [Authorize]
        public async Task<IActionResult> Play(int gameId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            int? gamerId = HttpContext.Session.GetInt32("GamerId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(role) || (!gamerId.HasValue && !userId.HasValue))
            {
                return RedirectToAction("Login", "Home");
            }

            var game = await _dbcontext.Games.FindAsync(gameId);
            if (game == null)
                return NotFound();

            // Check if the game requires payment
            if (game.Price > 0)
            {
                var payment = await _dbcontext.Payments
                    .FirstOrDefaultAsync(p => p.GameId == gameId &&
                                            (p.UserId == userId || p.GamerId == gamerId) &&
                                            p.Status == "Completed");

                if (payment == null)
                {
                    // Redirect to payment page if no payment found
                    return RedirectToAction("ProcessPayment", "Payment", new { gameId });
                }
            }

            var history = new GamerPlayinghistory
            {
                PremiumGameId = gameId,
                PlayedOn = DateTime.Now,
                GamerId = role == "Gamer" ? gamerId : null,
                UserId = role == "User" ? userId : null
            };

            _dbcontext.Playinghistorygamer.Add(history);
            await _dbcontext.SaveChangesAsync();

            return RedirectToAction("Details", "Launcher", new { id = gameId });
        }

        [Authorize]
        public IActionResult PlayingHistory()
        {
            var role = HttpContext.Session.GetString("UserRole");
            int? gamerId = HttpContext.Session.GetInt32("UserId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            IQueryable<GamerPlayinghistory> query = _dbcontext.Playinghistorygamer
                 .Include(h => h.PremiumGame)
                 .Include(h => h.Gamer)
                 .Include(h => h.User);

            if (role == "Gamer" && gamerId != null)
            {
                query = query.Where(h => h.GamerId == gamerId);
                ViewBag.Name = _dbcontext.Gamers
                    .Where(g => g.GamerId == gamerId)
                    .Select(g => g.Name)
                    .FirstOrDefault();
            }
            else if (role == "User" && userId != null)
            {
                query = query.Where(h => h.UserId == userId);
                ViewBag.Name = _dbcontext.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.Name)
                    .FirstOrDefault();
            }
            else if (role == "Admin")
            {
                ViewBag.Name = "All Players";
            }

            var history = query
                .OrderByDescending(h => h.PlayedOn)
                .ToList();

            if (!history.Any())
                ViewBag.NoHistory = "No playing history found.";

            return View(history);
        }

        [Authorize]
        public IActionResult DeleteHistory(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            int? gamerId = HttpContext.Session.GetInt32("UserId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            var history = _dbcontext.Playinghistorygamer.FirstOrDefault(h => h.PlayHistoryId == id);

            if (history == null)
                return NotFound();

            if (role == "Admin")
            {
                _dbcontext.Playinghistorygamer.Remove(history);
            }
            else if (role == "Gamer" && history.GamerId == gamerId)
            {
                _dbcontext.Playinghistorygamer.Remove(history);
            }
            else if (role == "User" && history.UserId == userId)
            {
                _dbcontext.Playinghistorygamer.Remove(history);
            }
            else
            {
                return Forbid();
            }

            _dbcontext.SaveChanges();

            return RedirectToAction("PlayingHistory");
        }
    }
}

//using System.Diagnostics;
//using System.Security.Claims;
//using GamaxyWebApplication.Data;
//using GamaxyWebApplication.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace GamaxyWebApplication.Controllers
//{
//    public class LauncherController : Controller
//    {
//        private string GenerateSlug(string title)
//        {
//            string slug = title.ToLowerInvariant();
//            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", ""); 
//            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();  
//            slug = slug.Substring(0, slug.Length <= 80 ? slug.Length : 80).Trim();          
//            slug = slug.Replace(" ", "-");

//            var baseSlug = slug;
//            int counter = 1;
//            while (_dbcontext.Games.Any(g => g.Slug == slug))
//            {
//                slug = $"{baseSlug}-{counter++}";
//            }

//            return slug;
//        }

//        private readonly ILogger<LauncherController> _logger;
//        private readonly ApplicationDbContext _dbcontext;
//        IWebHostEnvironment env;

//        public LauncherController(ILogger<LauncherController> logger, ApplicationDbContext db, IWebHostEnvironment env)
//        {
//            _logger = logger;
//            this._dbcontext = db;
//            this.env = env;
//        }
//        public IActionResult Index()
//        {
//            var userId = HttpContext.Session.GetInt32("UserId");
//            if (!userId.HasValue)
//            {
//                _logger.LogInformation("UserId is null in session. Redirecting to Home/Login.");
//                return RedirectToAction("Login", "Home");
//            }

//            var userEmail = User.Identity?.Name;
//            if (string.IsNullOrEmpty(userEmail))
//            {
//                _logger.LogWarning("User email is null or empty. Redirecting to Home/Login.");
//                return RedirectToAction("Login", "Home");
//            }

//            var games = _dbcontext.Games
//                .Include(g => g.Category)
//                .ToList();

//            var categories = _dbcontext.Categories.ToList();

//            var viewModel = new GameUploadViewModel
//            {
//                Games = games,
//                Categories = categories
//            };

//            return View(viewModel);
//        }


//        [Authorize(Roles = "Gamer")]
//        public async Task<IActionResult> LaunchingHistory()
//        {
//            int? gamerId = HttpContext.Session.GetInt32("UserId"); 
//            if (gamerId == null)
//            {
//                return RedirectToAction("Login", "Home");
//            }

//            var userEmail = User.Identity?.Name;
//            var uploadedGames = await _dbcontext.Games
//                .Where(g => g.UploadedBy == userEmail)   
//                .Include(g => g.Category)
//                .OrderByDescending(g => g.UploadedDate)
//                .ToListAsync();


//            if (!uploadedGames.Any())
//            {
//                ViewBag.NoGames = "You haven’t uploaded any games yet.";
//            }

//            return View(uploadedGames);
//        }

//        [HttpPost]
//        public async Task<IActionResult> MoveToRecycleBin(int id)
//        {
//            var game = await _dbcontext.Games.FindAsync(id);
//            if (game == null)
//                return NotFound();

//            game.IsDeleted = true; 
//            await _dbcontext.SaveChangesAsync();

//            return RedirectToAction("LaunchingHistory");
//        }


//        public async Task<IActionResult> RecycleBin()
//        {
//            var deletedGames = await _dbcontext.Games
//                .Where(g => g.IsDeleted)
//                .Include(g => g.Category) // optional
//                .OrderByDescending(g => g.UploadedDate)
//                .ToListAsync();

//            return View(deletedGames);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Recover(int id)
//        {
//            var game = await _dbcontext.Games.FindAsync(id);
//            if (game == null)
//                return NotFound();

//            game.IsDeleted = false; // 👈 restore
//            await _dbcontext.SaveChangesAsync();

//            return RedirectToAction("RecycleBin");
//        }

//        [HttpPost]
//        public async Task<IActionResult> PermanentDelete(int id)
//        {
//            var game = await _dbcontext.Games.FindAsync(id);
//            if (game == null)
//                return NotFound();

//            // remove associated files if needed
//            if (!string.IsNullOrEmpty(game.FilePath))
//            {
//                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", game.FilePath.TrimStart('/'));
//                if (System.IO.File.Exists(filePath))
//                    System.IO.File.Delete(filePath);
//            }
//            if (!string.IsNullOrEmpty(game.ImagePath))
//            {
//                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", game.ImagePath.TrimStart('/'));
//                if (System.IO.File.Exists(imagePath))
//                    System.IO.File.Delete(imagePath);
//            }

//            _dbcontext.Games.Remove(game);
//            await _dbcontext.SaveChangesAsync();

//            return RedirectToAction("RecycleBin");
//        }

//        [Authorize(Roles = "Gamer")]
//        public IActionResult MyGames()
//        {
//            var userEmail = User.Identity?.Name;

//            var viewModel = new GameUploadViewModel
//            {
//                Games = _dbcontext.Games
//                   .Where(g => g.UploadedBy == userEmail && !g.IsDeleted)
//                   .Include(g => g.Category)  
//                   .ToList(),
//                Categories = _dbcontext.Categories.ToList()
//            };
//            var games = _dbcontext.Games
//        .Select(g => new Game
//        {
//            GameId = g.GameId,
//            Title = g.Title,
//            Category = g.Category,
//            ImagePath = g.ImagePath,
//            ShortDescription = g.ShortDescription,
//            HowToPlay = g.HowToPlay,
//        })
//        .ToList();

//            return View(viewModel);
//        }

//        [Authorize]
//        [HttpPost]
//        public IActionResult RateGame(int gameId, int stars)
//        {
//            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (string.IsNullOrEmpty(userIdClaim))
//            {
//                return Json(new { success = false, message = "Unable to identify user." });
//            }

//            int userId = int.Parse(userIdClaim);
//            if (stars < 1 || stars > 5)
//            {
//                return Json(new { success = false, message = "Rating must be between 1 and 5." });
//            }

//            var game = _dbcontext.Games.FirstOrDefault(g => g.GameId == gameId);
//            if (game == null)
//            {
//                return Json(new { success = false, message = "Game not found." });
//            }

//            var existing = _dbcontext.PremiumGameRatings
//                .FirstOrDefault(r => r.GameId == gameId && r.UserId == userId);

//            if (existing != null)
//            {
//                existing.Rating = stars;
//                existing.RatingDate = DateTime.Now;
//                _dbcontext.Update(existing);
//            }
//            else
//            {
//                _dbcontext.PremiumGameRatings.Add(new PremiumGameRating
//                {
//                    UserId = userId,
//                    GameId = gameId,
//                    GameName = game.Title,
//                    Rating = stars,
//                    RatingDate = DateTime.Now
//                });
//            }

//            _dbcontext.SaveChanges();

//            return Json(new { success = true, message = "Rating submitted successfully!" });
//        }

//        [Authorize]
//        public IActionResult MyPremiumGameReviewsandRatings()
//        {
//            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (string.IsNullOrEmpty(userIdClaim))
//            {
//                return RedirectToAction("Login", "Home");
//            }

//            int userId = int.Parse(userIdClaim);
//            var ratings = _dbcontext.PremiumGameRatings
//                .Where(r => r.UserId == userId)
//                .OrderByDescending(r => r.RatingDate)
//                .ToList();

//            return View(ratings);
//        }



//        [Authorize(Roles = "Gamer")]
//        [HttpGet]
//        public IActionResult UploadGame()
//        {
//            ViewBag.CategoriesSl = new SelectList(_dbcontext.Categories, "CategoryId", "CategoryName");
//            return View();
//        }

//        [Authorize(Roles = "Gamer")]
//        [HttpPost]
//        public async Task<IActionResult> UploadGame(GameUploadViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                ViewBag.CategoriesSl = new SelectList(_dbcontext.Categories, "CategoryId", "CategoryName");
//                return View(model);
//            }

//            string gameFileName = Guid.NewGuid() + Path.GetExtension(model.GameFile.FileName);
//            string imageFileName = Guid.NewGuid() + Path.GetExtension(model.ThumbnailImage.FileName);

//            var gameFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploadedGames");
//            var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "gameThumbnails");

//            Directory.CreateDirectory(gameFolder);
//            Directory.CreateDirectory(imageFolder);

//            var gamePath = Path.Combine(gameFolder, gameFileName);
//            using (var stream = new FileStream(gamePath, FileMode.Create))
//            {
//                await model.GameFile.CopyToAsync(stream);
//            }

//            var imagePath = Path.Combine(imageFolder, imageFileName);
//            using (var stream = new FileStream(imagePath, FileMode.Create))
//            {
//                await model.ThumbnailImage.CopyToAsync(stream);
//            }

//            var game = new Game
//            {
//                Title = model.Title,
//                ShortDescription = model.ShortDescription,
//                LongDescription = model.LongDescription,
//                Price = model.Price,
//                FilePath = "/uploadedGames/" + gameFileName,
//                ImagePath = "/gameThumbnails/" + imageFileName,
//                UploadedDate = DateTime.Now,
//                CategoryId = model.CatId,
//                UploadSource = model.UploadMethod,
//                UploadedBy = User.Identity?.Name,
//                Slug = GenerateSlug(model.Title),
//                HowToPlay = model.HowToPlay 
//            };


//            _dbcontext.Games.Add(game);
//            await _dbcontext.SaveChangesAsync();

//            return RedirectToAction("MyGames");
//        }

//        [HttpGet("/Game/{slug}")]
//        public IActionResult Details(string slug)
//        {
//            if (string.IsNullOrWhiteSpace(slug))
//                return NotFound();

//            var game = _dbcontext.Games
//                .Include(g => g.Category)
//                .FirstOrDefault(g => g.Slug == slug);

//            if (game == null)
//                return NotFound();

//            return View(game);
//        }

//        [Authorize]
//        public IActionResult Play(int gameId)
//        {
//            var role = HttpContext.Session.GetString("Role"); 
//            int? gamerId = HttpContext.Session.GetInt32("GamerId");
//            int? userId = HttpContext.Session.GetInt32("UserId");

//            var history = new GamerPlayinghistory
//            {
//                PremiumGameId = gameId,
//                PlayedOn = DateTime.Now
//            };

//            if (role == "Gamer" && gamerId != null)
//                history.GamerId = gamerId;
//            else if (role == "User" && userId != null)
//                history.UserId = userId;

//            _dbcontext.Playinghistorygamer.Add(history);
//            _dbcontext.SaveChanges();

//            return RedirectToAction("Details", "Launcher", new { id = gameId });
//        }

//        [Authorize]
//        public IActionResult PlayingHistory()
//        {
//            var role = HttpContext.Session.GetString("Role");   
//            int? gamerId = HttpContext.Session.GetInt32("GamerId");
//            int? userId = HttpContext.Session.GetInt32("UserId");

//            IQueryable<GamerPlayinghistory> query = _dbcontext.Playinghistorygamer
//                 .Include(h => h.PremiumGame)
//                 .Include(h => h.Gamer)
//                 .Include(h => h.User);

//            if (role == "Gamer" && gamerId != null)
//            {
//                query = query.Where(h => h.GamerId == gamerId);
//                ViewBag.Name = _dbcontext.Gamers
//                    .Where(g => g.GamerId == gamerId)
//                    .Select(g => g.Name)
//                    .FirstOrDefault();
//            }
//            else if (role == "User" && userId != null)
//            {
//                query = query.Where(h => h.UserId == userId);
//                ViewBag.Name = _dbcontext.Users
//                    .Where(u => u.UserId == userId)
//                    .Select(u => u.Name)
//                    .FirstOrDefault();
//            }
//            else if (role == "Admin")
//            {
//                ViewBag.Name = "All Players";
//            }

//            // 🔹 Order latest first
//            var history = query
//                .OrderByDescending(h => h.PlayedOn)
//                .ToList();

//            if (!history.Any())
//                ViewBag.NoHistory = "No playing history found.";

//            return View(history);
//        }
//        [Authorize]
//        public IActionResult DeleteHistory(int id)
//        {
//            var role = HttpContext.Session.GetString("Role");
//            int? gamerId = HttpContext.Session.GetInt32("GamerId");
//            int? userId = HttpContext.Session.GetInt32("UserId");

//            var history = _dbcontext.Playinghistorygamer.FirstOrDefault(h => h.PlayHistoryId == id);

//            if (history == null)
//                return NotFound();


//            if (role == "Admin")
//            {
//                _dbcontext.Playinghistorygamer.Remove(history);
//            }

//            else if (role == "Gamer" && history.GamerId == gamerId)
//            {
//                _dbcontext.Playinghistorygamer.Remove(history);
//            }

//            else if (role == "User" && history.UserId == userId)
//            {
//                _dbcontext.Playinghistorygamer.Remove(history);
//            }
//            else
//            {
//                return Forbid();
//            }

//            _dbcontext.SaveChanges();

//            return RedirectToAction("PlayingHistory");
//        }


//    }
//}
