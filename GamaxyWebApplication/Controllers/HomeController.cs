using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using GamaxyWebApplication.Data;
using GamaxyWebApplication.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using GamaxyWebApplication.Services;
using Microsoft.AspNetCore.Authorization; // Added for [Authorize]

namespace GamaxyWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public HomeController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [Authorize]
        public IActionResult UserProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "User");
            if (user == null) return NotFound();

            return View(user);
        }

        [Authorize]
        public IActionResult GamerProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var gamer = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "Gamer");
            if (gamer == null) return NotFound();

            return View(gamer);
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditUserRegistrationDetails()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "User");
            if (user == null) return NotFound();

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditUserRegistrationDetails(Registration model, IFormFile? UserImage)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var existing = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "User");
            if (existing == null) return NotFound();

            // Check for duplicate Name (excluding current user)
            if (!string.IsNullOrEmpty(model.Name) && model.Name != existing.Name)
            {
                var existingName = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.Name == model.Name);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Name already exists. Please choose a different name.");
                    return View(model);
                }
            }

            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.Password = model.Password;
            existing.Age = model.Age;
            existing.Phone = model.Phone;
            existing.Address = model.Address;
            existing.Gender = model.Gender;

            // Handle image upload
            if (UserImage != null && UserImage.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);
                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(UserImage.FileName);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UserImage.CopyToAsync(stream);
                }
                existing.UserImage = "/uploads/" + uniqueFileName;
            }

            _context.Update(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditGamerRegistrationDetails()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var gamer = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "Gamer");
            if (gamer == null) return NotFound();

            return View(gamer);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditGamerRegistrationDetails(Registration model, IFormFile? UserImage)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var existing = _context.Registrationinfo.FirstOrDefault(r => r.RegistrationId == userId && r.Role == "Gamer");
            if (existing == null) return NotFound();

            // Check for duplicate Name (excluding current user)
            if (!string.IsNullOrEmpty(model.Name) && model.Name != existing.Name)
            {
                var existingName = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.Name == model.Name);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Name already exists. Please choose a different name.");
                    return View(model);
                }
            }

            // Check for duplicate GamerCode (excluding current user)
            if (!string.IsNullOrEmpty(model.GamerCode) && model.GamerCode != existing.GamerCode)
            {
                var existingGamerCode = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.GamerCode == model.GamerCode);
                if (existingGamerCode != null)
                {
                    ModelState.AddModelError("GamerCode", "GamerCode already exists. Please choose a different GamerCode.");
                    return View(model);
                }
            }

            existing.Name = model.Name;
            existing.Email = model.Email;
            existing.Password = model.Password;
            existing.Age = model.Age;
            existing.Phone = model.Phone;
            existing.Address = model.Address;
            existing.Gender = model.Gender;
            existing.GamerCode = model.GamerCode;

            // Handle image upload
            if (UserImage != null && UserImage.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);
                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(UserImage.FileName);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UserImage.CopyToAsync(stream);
                }
                existing.UserImage = "/uploads/" + uniqueFileName;
            }

            _context.Update(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction("GamerProfile");
        }


        [Authorize]
        [HttpPost]
        public IActionResult AddToFavorites(string gameName, string gameAction)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "Unable to identify user." });
            }

            int userId = int.Parse(userIdClaim);
            var favorite = new Favourite
            {
                UserId = userId,
                GameName = gameName,
                GameAction = gameAction
            };

            if (_context.Favourites.Any(f => f.UserId == userId && f.GameAction == gameAction))
            {
                return Json(new { success = false, message = "Already favorited" });
            }

            _context.Favourites.Add(favorite);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Favorites()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login");
            }

            int userId = int.Parse(userIdClaim);
            var favorites = _context.Favourites
                .Where(f => f.UserId == userId)
                .ToList();
            return View(favorites);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromFavorites(string gameAction)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "Unable to identify user." });
            }

            int userId = int.Parse(userIdClaim);
            var favorite = _context.Favourites
                .FirstOrDefault(f => f.UserId == userId && f.GameAction == gameAction);

            if (favorite == null)
            {
                return Json(new { success = false, message = "Game not found in favorites." });
            }

            _context.Favourites.Remove(favorite);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [Authorize]
        public IActionResult MyReviewsandRatings()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var userRatings = _context.FreeGameRatings
                .Where(r => r.UserId == userId)
                .Select(r => new GameRatingViewModel
                {
                    GameName = r.GameName,
                    UserId = r.UserId,
                    ExistingRating = r.Rating,
                    AverageRating = _context.FreeGameRatings
                        .Where(gr => gr.GameName == r.GameName)
                        .Average(gr => (double?)gr.Rating) ?? 0.0
                })
                .ToList();

            return View(userRatings);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userIdClaim) ? null : int.Parse(userIdClaim);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RateGame(string gameName, int rating)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Unable to identify user." });
            }

            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Rating must be between 1 and 5." });
            }

            var existingRating = _context.FreeGameRatings
                .FirstOrDefault(r => r.UserId == userId && r.GameName == gameName);

            if (existingRating != null)
            {
                existingRating.Rating = rating;
                existingRating.RatingDate = DateTime.Now;
                _context.Update(existingRating);
            }
            else
            {
                var newRating = new FreeGameRating
                {
                    UserId = userId.Value,
                    GameName = gameName,
                    Rating = rating
                };
                _context.FreeGameRatings.Add(newRating);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Rating submitted successfully!" });
        }



        public IActionResult Feedback()
        {
            var feedbackList = _context.Feedbacks
                                       .OrderByDescending(f => f.DateSubmitted)
                                       .ToList();
            return View(feedbackList);
        }

        public IActionResult Testimonials()
        {
            var feedbackList = _context.Feedbacks
                                       .OrderByDescending(f => f.DateSubmitted)
                                       .ToList();

            return View(feedbackList); 
        }

        [HttpPost]
        public IActionResult Submit(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.DateSubmitted = DateTime.Now;
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();

                TempData["Success"] = "Thanks for your feedback!";
                return RedirectToAction("Testimonials");
            }

            TempData["Error"] = "Please fill all fields.";
            return RedirectToAction("Testimonials"); 
        }
        public async Task<IActionResult> Index()
        {
            var players = await _context.LeaderboardPlayers
               .OrderByDescending(p => p.Level)
               .ThenByDescending(p => p.XP)    
               .ToListAsync();

            int rank = 1;
            foreach (var player in players)
            {
                player.Rank = rank++;
            }

            await _context.SaveChangesAsync();

            return View(players);
        }

        public IActionResult Games()
        {
            var averageRatings = _context.FreeGameRatings
                .GroupBy(r => r.GameName)
                .Select(g => new { GameName = g.Key, Average = g.Average(r => (double?)r.Rating) ?? 0.0 })
                .ToDictionary(g => g.GameName, g => g.Average);
            ViewBag.AverageRatings = averageRatings;
            return View();
        }
        public IActionResult Blog()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(Contact abc)
        {
            var isDuplicate = _context.Contactinfo.Any(c =>
                c.Name == abc.Name &&
                c.Email == abc.Email &&
                c.Subject == abc.Subject &&
                c.Message == abc.Message);

            if (!isDuplicate)
            {
                _context.Contactinfo.Add(abc);
                _context.SaveChanges();
            }

            return RedirectToAction("Contact");
        }


        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(Registration User, IFormFile? UserImage, string? GamerCode)
        {
            if (!ModelState.IsValid)
                return View(User);

            // Check for duplicate Email
            var existingUser = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.Email == User.Email);
            if (existingUser != null)
            {
                ViewBag.Message = "Email already exists.";
                return View(User);
            }

            // Check for duplicate Name
            if (!string.IsNullOrEmpty(User.Name))
            {
                var existingName = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.Name == User.Name);
                if (existingName != null)
                {
                    ViewBag.Message = "Name already exists. Please choose a different name.";
                    return View(User);
                }
            }

            // Check for duplicate GamerCode (if provided)
            if (!string.IsNullOrEmpty(GamerCode))
            {
                var existingGamerCode = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.GamerCode == GamerCode);
                if (existingGamerCode != null)
                {
                    ViewBag.Message = "GamerCode already exists. Please choose a different GamerCode.";
                    return View(User);
                }
            }

            // Handle Image Upload
            if (UserImage != null && UserImage.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);
                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(UserImage.FileName);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UserImage.CopyToAsync(stream);
                }

                User.UserImage = "/uploads/" + uniqueFileName;
            }
            else
            {
                User.UserImage = "/default-avatar.png";
            }

            // Generate Verification Code
            var verificationCode = new Random().Next(100000, 999999).ToString();
            User.VerificationCode = verificationCode;
            User.IsVerified = false;
            User.GamerCode = GamerCode;

            _context.Registrationinfo.Add(User);
            await _context.SaveChangesAsync();

            // Create corresponding User or Gamer record
            if (User.Role == "User")
            {
                var newUser = new User
                {
                    RegistrationId = User.RegistrationId,
                    Name = User.Name,
                    Email = User.Email,
                    Password = User.Password,
                    Gender = User.Gender,
                    Age = User.Age,
                    Phone = User.Phone,
                    Address = User.Address,
                    UserImage = User.UserImage,
                    JoiningDate = DateTime.Now
                };
                _context.Users.Add(newUser);
            }
            else if (User.Role == "Gamer")
            {
                var newGamer = new Gamer
                {
                    Name = User.Name,
                    Email = User.Email,
                    Password = User.Password,
                    Gender = User.Gender,
                    Age = User.Age,
                    Phone = User.Phone,
                    Address = User.Address,
                    UserImage = User.UserImage,
                    GamerCode = User.GamerCode
                };
                _context.Gamers.Add(newGamer);
            }

            await _context.SaveChangesAsync();

            // Send verification email
            _emailService.SendVerificationEmail(User.Email, verificationCode);

            return RedirectToAction("VerifyEmail", new { email = User.Email });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            var user = await _context.Registrationinfo.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && user.VerificationCode == code)
            {
                user.IsVerified = true;
                user.VerificationCode = null; // Clear after verification
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Invalid verification code.";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string? Email, string? Password, string? ReturnUrl = null)
        {
            if (Email?.ToLower() == "gamaxyinfo@gmail.com" && Password == "game123")
            {
                // Admin hardcoded login
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "GamaxyAdmin"),
            new Claim(ClaimTypes.Email, Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.NameIdentifier, "0")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetInt32("UserId", 0);

                return RedirectToLocal(ReturnUrl ?? "/Admin/Index");
            }

            var ex = _context.Registrationinfo.FirstOrDefault(u => u.Email == Email && u.Password == Password);
            if (ex != null)
            {
                if (!ex.IsVerified)
                {
                    ViewBag.Error = "Please verify your email before logging in.";
                    return View();
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, ex.Name),
            new Claim(ClaimTypes.Email, ex.Email),
            new Claim(ClaimTypes.NameIdentifier, ex.RegistrationId.ToString()),
            new Claim(ClaimTypes.Role, ex.Role)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetInt32("UserId", ex.RegistrationId);
                HttpContext.Session.SetString("UserRole", ex.Role);

                // Set GamerId or UserId based on role
                if (ex.Role == "Gamer")
                {
                    var gamer = await _context.Gamers.FirstOrDefaultAsync(g => g.Email == Email);
                    if (gamer != null)
                    {
                        HttpContext.Session.SetInt32("GamerId", gamer.GamerId);
                    }
                }
                else if (ex.Role == "User")
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                    if (user != null)
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                    }
                }

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    return Redirect(ReturnUrl);

                if (ex.Role == "User")
                    return RedirectToAction("UserProfile", "Home");

                if (ex.Role == "Gamer")
                    return RedirectToAction("GamerProfile", "Home");

            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Registrationinfo.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Error = "Email not found!";
                return View();
            }

            // Hash the new password before saving
            user.Password = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password updated successfully! You can now login.";
            return View();
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public async Task<IActionResult> Leaderboard()
        {
            var players = await _context.LeaderboardPlayers
                .OrderByDescending(p => p.Level) 
                .ThenByDescending(p => p.XP)     
                .ToListAsync();

            int rank = 1;
            foreach (var player in players)
            {
                player.Rank = rank++;
            }

            await _context.SaveChangesAsync();

            return View(players);
        }
      
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
        }

        public IActionResult Ppolicy()
        {
            return View();
        }

        public IActionResult Faqs()
        {
            return View();
        }

        public IActionResult Imgallery()
        {
            return View();
        }
       
        public IActionResult videogallery()
        {
            return View();
        }

        public IActionResult TermsandConditions()
        {
            return View();
        }

        public IActionResult FeedbackForm()
        {
            return View();
        }

        public IActionResult SearchResult()
        {
            return View();
        }

        public IActionResult AeroplaneFight()
        {
            return GetGameViewModel("Aeroplane Fight");
        }
        public IActionResult AlienDefense()
        {
            return GetGameViewModel("Alien Defense");
        }
        public IActionResult AngryBird()
        {
            return GetGameViewModel("Angry Bird");
        }
        public IActionResult Ballon()
        {
            return GetGameViewModel("Ballon");
        }
        public IActionResult bubbleshooter()
        {
            return GetGameViewModel("Bubble Shooter");
        }
        public IActionResult burger()
        {
            return GetGameViewModel("Burger");
        }
        public IActionResult CandyCrushGame()
        {
            return GetGameViewModel("Candy Crush");
        }
        public IActionResult CarEscape()
        {
            return GetGameViewModel("Car Escape");
        }
        public IActionResult CarParker()
        {
            return GetGameViewModel("Car Parker");
        }
        public IActionResult chess()
        {
            return GetGameViewModel("Chess");
        }
        public IActionResult ChronoRunner()
        {
            return GetGameViewModel("Chrono Runner");
        }
        public IActionResult cosmicdefender()
        {
            return GetGameViewModel("Cosmic Defender");
        }
        public IActionResult CrazyCarRun()
        {
            return GetGameViewModel("Crazy Car Run");
        }
        public IActionResult CrossyRoadGame()
        {
            return GetGameViewModel("CrossyRoad Game");
        }
        public IActionResult CutTheRope()
        {
            return GetGameViewModel("Cut The Rope");
        }
        public IActionResult delivery()
        {
            return GetGameViewModel("Delivery");
        }
        public IActionResult DropEscape()
        {
            return GetGameViewModel("Drop Escape");
        }
        public IActionResult Drumkit()
        {
            return GetGameViewModel("Drumkit");
        }
        public IActionResult fightinggame()
        {
            return GetGameViewModel("Fighting Game");
        }
        public IActionResult FireBoyandWaterGirl()
        {
            return GetGameViewModel("Fire & Water");
        }
        public IActionResult fish()
        {
            return GetGameViewModel("Fish");
        }
        public IActionResult FlappyBird()
        {

            return GetGameViewModel("Flappy Bird");
        }
        public IActionResult Flipcard()
        {
            return GetGameViewModel("Flip Card");
        }
        public IActionResult FruitNinja()
        {
            return GetGameViewModel("Fruit Ninja");
        }
        public IActionResult Hangmen()
        {
            return GetGameViewModel("Hangmen");
        }
        public IActionResult helicopter()
        {
            return GetGameViewModel("Helicopter");
        }
        public IActionResult matchthecolumn()
        {
            return GetGameViewModel("Match the Column");
        }
        public IActionResult Memorygame()
        {
            return GetGameViewModel("Memory Game");
        }
        public IActionResult NeonRacer()
        {
            return GetGameViewModel("Neon Racer");
        }
        public IActionResult Pacman()
        {
            return GetGameViewModel("Pacman");
        }
        public IActionResult Pongball()
        {
            return GetGameViewModel("Pong Ball");
        }
        public IActionResult RockPaperScissor2()
        {
            return GetGameViewModel("Rock Paper Scissor 2");
        }
        public IActionResult RockPaperScissors()
        {
            return GetGameViewModel("Rock Paper Scissors");
        }
        public IActionResult Runningman()
        {
            return GetGameViewModel("Running Man");
        }
        public IActionResult SimonSaysGame()
        {
            return GetGameViewModel("Simon Says Game");
        }
        public IActionResult snake()
        {
            return GetGameViewModel("Snake");
        }
        public IActionResult TankShooter()
        {
            return GetGameViewModel("Tank Shooter");
        }
        public IActionResult tetris()
        {
            return GetGameViewModel("Tetris");
        }
        public IActionResult Thinking()
        {
            return GetGameViewModel("Thinking");
        }
        public IActionResult TicTacToe()
        {
            return GetGameViewModel("Tic Tac Toe");
        }
        public IActionResult TowerBlocks()
        {
            return GetGameViewModel("Tower Blocks");
        }
        public IActionResult UltimateBrickBreaker()
        {
            return GetGameViewModel("Ultimate Brick Breaker");
        }
        public IActionResult waterflow()
        {
            return GetGameViewModel("water flow");
        }
        public IActionResult WhackAMole()
        {
            return GetGameViewModel("Whack A Mole");
        }
        private IActionResult GetGameViewModel(string gameName)
        {
            var userId = GetCurrentUserId();
            var model = new GameRatingViewModel
            {
                GameName = gameName,
                UserId = userId,
                ExistingRating = userId.HasValue ? _context.FreeGameRatings
                    .Where(r => r.UserId == userId && r.GameName == gameName)
                    .Select(r => r.Rating)
                    .FirstOrDefault() : 0,
                AverageRating = _context.FreeGameRatings
                    .Where(r => r.GameName == gameName)
                    .Average(r => (double?)r.Rating) ?? 0.0
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public IActionResult Play(string gameAction, int? gameId = null, bool isPremium = false)
        {
            var role = HttpContext.Session.GetString("Role");
            int? gamerId = HttpContext.Session.GetInt32("GamerId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            var history = new GamerPlayinghistory
            {
                GamerId = role == "Gamer" ? gamerId : null,
                UserId = role == "User" ? userId : null,
                PlayedOn = DateTime.Now
            };

            if (isPremium && gameId.HasValue)
            {
                // ✅ Dynamic Premium Game
                history.PremiumGameId = gameId.Value;
            }
            else
            {
                // ✅ Static Game
                history.StaticGameName = gameAction; // e.g. "bubbleshooter", "balloon"
            }

            _context.Playinghistorygamer.Add(history);
            _context.SaveChanges();

            ViewBag.GameAction = gameAction;
            ViewBag.GameId = gameId;
            ViewBag.IsPremium = isPremium;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> PlayingHistory()
        {
            var role = HttpContext.Session.GetString("Role");
            int? gamerId = HttpContext.Session.GetInt32("GamerId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            IQueryable<GamerPlayinghistory> query = _context.Playinghistorygamer
              .Include(h => h.PremiumGame)
              .Include(h => h.Gamer)
              .Include(h => h.User);

            if (role == "Gamer")
            {
                if (gamerId == null) return Unauthorized();
                query = query.Where(h => h.GamerId == gamerId);
                ViewBag.Name = await _context.Gamers
                    .Where(g => g.GamerId == gamerId)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync();
            }
            else if (role == "User")
            {
                if (userId == null) return Unauthorized();
                query = query.Where(h => h.UserId == userId);
                ViewBag.Name = await _context.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync();
            }
            else if (role == "Admin")
            {
                ViewBag.Name = "All Players";
            }

            var history = await query
                .OrderByDescending(h => h.PlayedOn)
                .ToListAsync();

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

            var history = _context.Playinghistorygamer.FirstOrDefault(h => h.PlayHistoryId == id);

            if (history == null)
                return NotFound();

            if (role == "Admin" ||
                (role == "Gamer" && history.GamerId == gamerId) ||
                (role == "User" && history.UserId == userId))
            {
                _context.Playinghistorygamer.Remove(history);
                _context.SaveChanges();
                return RedirectToAction("PlayingHistory");
            }

            return Forbid();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
