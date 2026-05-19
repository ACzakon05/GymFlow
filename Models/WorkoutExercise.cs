using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFlow.Models
{
    /// <summary>
    /// Model relacji N:M między Workout i Exercise
    /// Reprezentuje ćwiczenie dodane do treningu z dodatkowymi informacjami
    /// </summary>
    public class WorkoutExercise
    {
        /// <summary>
        /// Unikalny identyfikator (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identyfikator treningu - klucz obcy do Workout
        /// </summary>
        [ForeignKey(nameof(Workout))]
        [Required]
        public int WorkoutId { get; set; }

        /// <summary>
        /// Identyfikator ćwiczenia - klucz obcy do Exercise
        /// </summary>
        [ForeignKey(nameof(Exercise))]
        [Required]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Liczba serii (setów)
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Liczba serii")]
        public int Sets { get; set; }

        /// <summary>
        /// Liczba powtórzeń na serię
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Liczba powtórzeń")]
        public int Reps { get; set; }

        /// <summary>
        /// Opcjonalny ciężar (w kg)
        /// </summary>
        [Range(0, 500)]
        [Display(Name = "Ciężar (kg)")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// Opcjonalny odpoczynek między seriami (w sekundach)
        /// </summary>
        [Range(0, 600)]
        [Display(Name = "Odpoczynek (s)")]
        public int? RestSeconds { get; set; }

        /// <summary>
        /// Opcjonalne notatki do tego ćwiczenia
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Notatki")]
        public string? Notes { get; set; }

        /// <summary>
        /// Kolejność ćwiczenia w treningu
        /// </summary>
        [Required]
        [Range(1, 1000)]
        [Display(Name = "Kolejność")]
        public int Order { get; set; }

        // ========== RELACJE (NAWIGACJA) ==========

        /// <summary>
        /// Trening do którego należy to ćwiczenie
        /// </summary>
        public virtual Workout? Workout { get; set; }

        /// <summary>
        /// Ćwiczenie zawarte w treningu
        /// </summary>
        public virtual Exercise? Exercise { get; set; }
    }
}
