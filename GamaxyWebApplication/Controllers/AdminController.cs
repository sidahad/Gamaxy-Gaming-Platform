using System;
using System.Diagnostics;
using GamaxyWebApplication.Data;
using GamaxyWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GamaxyWebApplication.Controllers
{
    public class AdminController : Controller
    {
        private const int PageSize = 10;
        private string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace(".", "").Replace("'", "").Replace(",", "");
        }
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _dbcontext;
        IWebHostEnvironment env;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext dbcontext, IWebHostEnvironment env)
        {
            _logger = logger;
            this._dbcontext = dbcontext;
            this.env = env;
        }
     
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _dbcontext.Categories.Add(category);
            _dbcontext.SaveChanges();
            return RedirectToAction("ViewCategories");
        }

        public IActionResult ViewCategories()
        {
            var categories = _dbcontext.Categories.ToList();
            return View(categories);
        }
        public IActionResult UsersList()
        {
            var users = _dbcontext.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _dbcontext.Users.FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                _dbcontext.Users.Remove(user);
                _dbcontext.SaveChanges();
            }
            return RedirectToAction("UsersList");
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Home");
            }

            var totalUsers = _dbcontext.Users.Count();
            var totalGamers = _dbcontext.Gamers.Count();
            var totalCategories = _dbcontext.Categories.Count();
            var totalGames = _dbcontext.Games.Count();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalGamers = totalGamers;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalGames = totalGames;

            return View();
        }
      
        public IActionResult PlayingHistory()
        {
            var role = HttpContext.Session.GetString("Role");
            int? gamerId = HttpContext.Session.GetInt32("GamerId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            IQueryable<GamerPlayinghistory> query = _dbcontext.Playinghistorygamer
            .Include(h => h.PremiumGame)
            .Include(h => h.Gamer)
            .Include(h => h.User);

            if (role == "Gamer" && gamerId != null)
            {
                query = query.Where(h => h.GamerId == gamerId);
                ViewBag.Name = _dbcontext.Gamers.Where(g => g.GamerId == gamerId).Select(g => g.Name).FirstOrDefault();
            }
            else if (role == "User" && userId != null)
            {
                query = query.Where(h => h.UserId == userId);
                ViewBag.Name = _dbcontext.Users.Where(u => u.UserId == userId).Select(u => u.Name).FirstOrDefault();
            }
            else if (role == "Admin")
            {
                ViewBag.Name = "All Players";
            }

            var history = query.OrderByDescending(h => h.PlayedOn).ToList();

            if (!history.Any())
                ViewBag.NoHistory = "No playing history found.";

            return View(history);
        }

        [Authorize]
        public IActionResult DeleteHistory(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            int? gamerId = HttpContext.Session.GetInt32("GamerId");
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

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult ManageUsers() //Add/edit/block users
        {
            return View();
        }
        public IActionResult ManageGames() //Upload/edit/delete games
        {
            return View();
        }
        public IActionResult ManageCategories()  //Add/remove categories
        {
            return View();
        }
        public IActionResult ManageReviews() //Approve/delete user reviews
        {
            return View();
        }
        public IActionResult SiteSettings()  //Logo, contact email, theme colors
        {
            return View();
        }
        public IActionResult Analytics()   //Traffic, downloads, game popularity charts
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }


        [HttpGet]
        public IActionResult Contactinfo()
        {
            var contacts = _dbcontext.Contactinfo.ToList();

            var duplicates = contacts
                .GroupBy(c => new { c.Name, c.Email, c.Subject })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Select(c => c.Id)
                .ToList();

            ViewBag.DuplicateIds = duplicates;
            return View(contacts);
        }


        [HttpGet]
        public IActionResult Editcon(int? Id)
        {
            return View(_dbcontext.Contactinfo.Find(Id));
        }

        [HttpPost]
        public IActionResult Editcon(Contact abc)
        {
            _dbcontext.Contactinfo.Update(abc);
            _dbcontext.SaveChanges();
            return RedirectToAction("Contactinfo");
        }

        public IActionResult delcon(int? Id)
        {

            var o = _dbcontext.Contactinfo.Find(Id);
            _dbcontext.Contactinfo.Remove(o);
            _dbcontext.SaveChanges();
            return RedirectToAction("Contactinfo");
        }

        public async Task<IActionResult> ContactList()
        {
            var enquiries = await _dbcontext.Contactinfo.ToListAsync();
            return View(enquiries);
        }

        public async Task<IActionResult> LeaderboardView()
        {
            var players = await _dbcontext.LeaderboardPlayers
                .OrderByDescending(p => p.Level) 
                .ThenByDescending(p => p.XP)     
                .ToListAsync();

            int rank = 1;
            foreach (var player in players)
            {
                player.Rank = rank++;
            }

            await _dbcontext.SaveChangesAsync();

            return View(players);
        }

        [HttpGet]
        public async Task<IActionResult> LeaderboardForm(int? id)
        {
            if (id == null) return View(new LeaderboardPlayer());

            var player = await _dbcontext.LeaderboardPlayers.FindAsync(id);
            if (player == null) return NotFound();

            return View(player);
        }

        [HttpPost]
        public async Task<IActionResult> LeaderboardForm(LeaderboardPlayer player)
        {
            if (!ModelState.IsValid)
                return View(player);

            if (player.Id == 0)
                _dbcontext.LeaderboardPlayers.Add(player);
            else
                _dbcontext.LeaderboardPlayers.Update(player);

            await _dbcontext.SaveChangesAsync();
            return RedirectToAction("LeaderboardView");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var player = await _dbcontext.LeaderboardPlayers.FindAsync(id);
            if (player != null)
            {
                _dbcontext.LeaderboardPlayers.Remove(player);
                await _dbcontext.SaveChangesAsync();
            }
            return RedirectToAction("LeaderboardView");
        }
        [HttpGet]
        public IActionResult FeedbackList()
        {
            var feedbackList = _dbcontext.Feedbacks
                                       .OrderByDescending(f => f.DateSubmitted)
                                       .ToList();
            return View(feedbackList);
        }

        [HttpPost]
        public IActionResult Feedbackdelete(int id)
        {
            var feedback = _dbcontext.Feedbacks.Find(id);
            if (feedback != null)
            {
                _dbcontext.Feedbacks.Remove(feedback);
                _dbcontext.SaveChanges();
                TempData["Success"] = "Feedback deleted successfully.";
            }
            return RedirectToAction("AdminList");
        }
      
        
        public async Task<IActionResult> GamerList()
        {
            var gamers = await _dbcontext.Gamers.ToListAsync();
            return View(gamers);
        }

       
        [HttpPost]
        public async Task<IActionResult> DeleteGamer(int id)
        {
            var gamer = await _dbcontext.Gamers.FindAsync(id);
            if (gamer != null)
            {
                _dbcontext.Gamers.Remove(gamer);
                await _dbcontext.SaveChangesAsync();
            }
            return RedirectToAction("GamerList");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Games()
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



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddGame()
        {
            ViewBag.CategoriesSl = new SelectList(_dbcontext.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddGame(GameUploadViewModel model)
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

            return RedirectToAction("Games");
        }

    }
}
