using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GymFlow.Controllers
{
    /// <summary>
    /// Kontroler obsługujący autoryzację - logowanie i rejestrację użytkowników
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Konstruktor - dependency injection DbContext
        /// </summary>
        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========== REJESTRACJA ==========

        /// <summary>
        /// GET - wyświetla formularz rejestracji
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST - obsługuje rejestrację nowego użytkownika
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Walidacja modelu
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sprawdzenie czy użytkownik już istnieje
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Użytkownik z taką nazwą lub emailem już istnieje!");
                return View(model);
            }

            try
            {
                // Tworzenie nowego użytkownika
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    // Hashowanie hasła za pomocą BCrypt
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = UserRole.User, // Domyślna rola
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    // Generowanie API Key (dla REST API)
                    ApiKey = Guid.NewGuid().ToString()
                };

                // Dodanie do bazy danych
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nowy użytkownik zarejestrowany: {user.Username}");

                // Automatyczne logowanie po rejestracji
                await LoginUser(user);

                TempData["Success"] = "Pomyślnie zarejestrowano! Witaj w GymFlow!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas rejestracji: {ex.Message}");
                ModelState.AddModelError("", "Błąd podczas rejestracji. Spróbuj ponownie.");
                return View(model);
            }
        }

        // ========== LOGOWANIE ==========

        /// <summary>
        /// GET - wyświetla formularz logowania
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST - obsługuje logowanie użytkownika
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // Walidacja modelu
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Szukanie użytkownika po nazwie użytkownika LUB emailu
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                // Sprawdzenie czy użytkownik istnieje i hasło jest poprawne
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika/email lub hasło!");
                    return View(model);
                }

                // Sprawdzenie czy konto jest aktywne
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Twoje konto zostało dezaktywowane!");
                    return View(model);
                }

                // Logowanie użytkownika
                await LoginUser(user);

                _logger.LogInformation($"Użytkownik zalogowany: {user.Username}");

                TempData["Success"] = $"Witaj, {user.FirstName ?? user.Username}!";

                // Jeśli jest returnUrl, przekieruj tam. W przeciwnym razie do Home
                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas logowania: {ex.Message}");
                ModelState.AddModelError("", "Błąd podczas logowania. Spróbuj ponownie.");
                return View(model);
            }
        }

        // ========== WYLOGOWANIE ==========

        /// <summary>
        /// POST - obsługuje wylogowanie użytkownika
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Wylogowanie z cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Info"] = "Pomyślnie wylogowano!";
            return RedirectToAction("Index", "Home");
        }

        // ========== METODY POMOCNICZE ==========

        /// <summary>
        /// Loguje użytkownika - tworzy cookie authentication
        /// </summary>
        private async Task LoginUser(User user)
        {
            // Tworzenie Claims (informacje o użytkowniku)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Tworzenie identity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Tworzenie principal
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Zapamiętaj logowanie
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // Ważne przez 30 dni
            };

            // Logowanie przez cookies
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        /// <summary>
        /// Bezpieczne przekierowanie - sprawdza czy URL jest lokalny
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }

    // ========== VIEW MODELS ==========

    /// <summary>
    /// Model do rejestracji
    /// </summary>
    public class RegisterViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Imię")]
        public string? FirstName { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nazwisko")]
        public string? LastName { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Potwierdź hasło")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Hasła się nie zgadzają!")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model do logowania
    /// </summary>
    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Nazwa użytkownika lub email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; } = true;
    }
}
