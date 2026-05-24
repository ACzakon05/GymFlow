using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;

namespace GymFlow.Controllers
{
    /// <summary>
    /// Kontroler panelu administratora - zarządzanie użytkownikami
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // ========== DASHBOARD - STATYSTYKI ==========

        /// <summary>
        /// GET - wyświetla dashboard administratora z statystykami
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            var inactiveUsers = await _context.Users.CountAsync(u => !u.IsActive);
            var totalWorkouts = await _context.Workouts.CountAsync();
            var totalProgressEntries = await _context.ProgressEntries.CountAsync();

            var stats = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                AdminCount = adminCount,
                InactiveUsers = inactiveUsers,
                TotalWorkouts = totalWorkouts,
                TotalProgressEntries = totalProgressEntries,
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        // ========== INDEX - LISTA UŻYTKOWNIKÓW ==========

        /// <summary>
        /// GET - wyświetla listę wszystkich użytkowników
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm = null)
        {
            IQueryable<User> query = _context.Users.OrderBy(u => u.Username);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            var users = await query.ToListAsync();
            ViewBag.SearchTerm = searchTerm;

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int? id)
        {   
            if (id==null) return NotFound();
            var user=await _context.Users.FindAsync(id);
            if (user==null) return NotFound();
            return View(user);
        }

        // ========== EDIT USER - EDYCJA UŻYTKOWNIKA ==========

        /// <summary>
        /// GET - wyświetla formularz edycji użytkownika
        /// </summary>
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        /// <summary>
        /// POST - aktualizuje dane użytkownika
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.IsActive;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Dane użytkownika zostały zaktualizowane!";
                    return RedirectToAction(nameof(UserDetails), new { id });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Błąd przy aktualizacji użytkownika: {ex.Message}");
                    TempData["Error"] = "Błąd przy aktualizacji danych użytkownika.";
                    return RedirectToAction(nameof(UserDetails), new { id });
                }
            }
            return View(model);
        }

        /// <summary>
        /// POST - usuwa użytkownika (admin nie może usunąć siebie)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int currentUserId = GetCurrentUserId();

            // Zabezpieczenie: admin nie może usunąć siebie
            if (id == currentUserId)
            {
                TempData["Error"] = "Nie możesz usunąć swojego własnego konta!";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Użytkownik oraz wszystkie jego dane zostały usunięte!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy usuwaniu użytkownika: {ex.Message}");
                TempData["Error"] = "Błąd przy usuwaniu użytkownika.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }
        }

        // ========== RESET PASSWORD - RESETOWANIE HASŁA ==========

        /// <summary>
        /// POST - resetuje hasło użytkownika na domyślne
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            try
            {
                // Resetuj hasło na "Password123!"
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hasło użytkownika zostało zresetowane na: Password123!";
                return RedirectToAction(nameof(UserDetails), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy resetowaniu hasła: {ex.Message}");
                TempData["Error"] = "Błąd przy resetowaniu hasła.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }
        }

        // ========== TOGGLE ACTIVE - AKTYWACJA/DEAKTYWACJA ==========

        /// <summary>
        /// POST - przełącza status aktywności użytkownika
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            int currentUserId = GetCurrentUserId();

            // Zabezpieczenie: admin nie może deaktywować siebie
            if (id == currentUserId)
            {
                TempData["Error"] = "Nie możesz zmienić statusu swojego własnego konta!";
                return RedirectToAction(nameof(UserDetails), new { id });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            try
            {
                user.IsActive = !user.IsActive;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Konto użytkownika zostało {(user.IsActive ? "aktywowane" : "deaktywowane")}!";
                return RedirectToAction(nameof(UserDetails), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy zmianie statusu użytkownika: {ex.Message}");
                TempData["Error"] = "Błąd przy zmianie statusu.";
                return RedirectToAction(nameof(UserDetails), new { id });
            }
        }
    }

    // ========== VIEW MODELS ==========

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalWorkouts { get; set; }
        public int TotalProgressEntries { get; set; }
        public List<User> RecentUsers { get; set; } = new();
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsActive { get; set; } = true;
    }
}