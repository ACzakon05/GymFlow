using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFlow.Models
{
    /// <summary>
    /// Model wpisu postępu - śledzenie jak użytkownik wykonał trening
    /// </summary>
    public class ProgressEntry
    {
        /// <summary>
        /// Unikalny identyfikator wpisu (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identyfikator użytkownika - klucz obcy do User
        /// </summary>
        [ForeignKey(nameof(User))]
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Identyfikator ćwiczenia - klucz obcy do Exercise
        /// </summary>
        [ForeignKey(nameof(Exercise))]
        [Required]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Data wykonania ćwiczenia
        /// </summary>
        [Required]
        [Display(Name = "Data")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Liczba wykonanych serii
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Wykonane serie")]
        public int CompletedSets { get; set; }

        /// <summary>
        /// Liczba wykonanych powtórzeń
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Wykonane powtórzenia")]
        public int CompletedReps { get; set; }

        /// <summary>
        /// Ciężar który użytkownik podniósł (w kg)
        /// </summary>
        [Range(0, 500)]
        [Display(Name = "Ciężar (kg)")]
        public decimal? ActualWeight { get; set; }

        /// <summary>
        /// Czy ćwiczenie zostało w pełni ukończone
        /// </summary>
        [Display(Name = "Ukończone")]
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Opcjonalne notatki z treningu
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Notatki")]
        public string? Notes { get; set; }

        /// <summary>
        /// Opór (poziom trudności) - np. dla maszyn
        /// </summary>
        [StringLength(50)]
        public string? Resistance { get; set; }

        // ========== RELACJE (NAWIGACJA) ==========

        /// <summary>
        /// Użytkownik który wykonał ćwiczenie
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Ćwiczenie które zostało wykonane
        /// </summary>
        public virtual Exercise? Exercise { get; set; }
    }
}
