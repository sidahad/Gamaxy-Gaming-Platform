using System;
using System.Threading.Tasks;
using GamaxyWebApplication.Data;
using GamaxyWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamaxyWebApplication.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _dbcontext;

        public PaymentController(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int gameId)
        {
            var game = await _dbcontext.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Home");
            }

            int registrationId = int.Parse(userIdClaim);
            var role = HttpContext.Session.GetString("UserRole");

            // Get UserId or GamerId based on role
            int? userId = null;
            int? gamerId = null;
            if (role == "User")
            {
                var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.RegistrationId == registrationId);
                if (user != null)
                {
                    userId = user.UserId;
                }
            }
            else if (role == "Gamer")
            {
                var gamer = await _dbcontext.Gamers.FirstOrDefaultAsync(g => g.Email == User.Identity.Name);
                if (gamer != null)
                {
                    gamerId = gamer.GamerId;
                }
            }

            // Check if payment already exists
            var existingPayment = await _dbcontext.Payments
                .FirstOrDefaultAsync(p => p.GameId == gameId &&
                                        (p.UserId == userId || p.GamerId == gamerId) &&
                                        p.Status == "Completed");

            if (existingPayment != null)
            {
                // Payment already made, redirect to play
                return RedirectToAction("MyGames", "Launcher", new { gameId });
            }

            var model = new PaymentViewModel
            {
                GameId = gameId,
                GameTitle = game.Title,
                Amount = game.Price ?? 0,
                UserId = registrationId,
                Role = role
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Home");
            }

            int registrationId = int.Parse(userIdClaim);
            var game = await _dbcontext.Games.FindAsync(model.GameId);
            if (game == null)
            {
                return NotFound();
            }

            // Get UserId or GamerId based on role
            int? userId = null;
            int? gamerId = null;
            if (model.Role == "User")
            {
                var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.RegistrationId == registrationId);
                if (user != null)
                {
                    userId = user.UserId;
                }
            }
            else if (model.Role == "Gamer")
            {
                var gamer = await _dbcontext.Gamers.FirstOrDefaultAsync(g => g.Email == User.Identity.Name);
                if (gamer != null)
                {
                    gamerId = gamer.GamerId;
                }
            }

            // Simulate payment processing (replace with actual payment gateway integration)
            var payment = new Payment
            {
                UserId = userId,
                GamerId = gamerId,
                GameId = model.GameId,
                Amount = model.Amount,
                Status = "Completed", // In production, set to "Pending" until gateway confirms
                PaymentDate = DateTime.Now,
                TransactionId = Guid.NewGuid().ToString() // Placeholder for payment gateway transaction ID
            };

            _dbcontext.Payments.Add(payment);
            await _dbcontext.SaveChangesAsync();

            // Redirect to play the game
            return RedirectToAction("MyGames", "Launcher", new { gameId = model.GameId });
        }
    }
}