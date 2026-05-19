using System.ComponentModel.DataAnnotations;

namespace GymFlow.Models
{
    /// <summary>
    /// Model użytkownika w systemie
    /// </summary>
    public class User
    {
        // ========== WŁAŚCIWOŚCI PODSTAWOWE ==========
        
        /// <summary>
        /// Unikalny identyfikator użytkownika (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa użytkownika - unikalna
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email użytkownika - unikalny
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash hasła (nigdy nie przechowujemy hasła w czystej postaci!)
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Imię użytkownika
        /// </summary>
        [StringLength(100)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Nazwisko użytkownika
        /// </summary>
        [StringLength(100)]
        public string? LastName { get; set; }

        /// <summary>
        /// Rola użytkownika (Admin lub User)
        /// </summary>
        [Required]
        [Display(Name = "Rola")]
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Data utworzenia konta
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Czy konto jest aktywne
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ========== RELACJE (NAWIGACJA) ==========

        /// <summary>
        /// Treningi należące do tego użytkownika (1 użytkownik : wiele treningów)
        /// </summary>
        public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();

        /// <summary>
        /// Wpisy postępu należące do tego użytkownika
        /// </summary>
        public virtual ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();

        /// <summary>
        /// Klucz API do autoryzacji w REST API
        /// </summary>
        public string? ApiKey { get; set; }
    }

    /// <summary>
    /// Enum do określenia roli użytkownika
    /// </summary>
    public enum UserRole
    {
        /// <summary>Zwykły użytkownik - może tworzyć swoje treningi</summary>
        User = 0,
        /// <summary>Administrator - pełny dostęp do systemu</summary>
        Admin = 1
    }
}
