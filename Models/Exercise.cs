using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFlow.Models
{
    /// <summary>
    /// Model ćwiczenia - np. "Wyciskanie na ławce", "Martwary ciąg"
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Unikalny identyfikator ćwiczenia (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa ćwiczenia - wymagana
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Nazwa ćwiczenia")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Opcjonalny opis jak wykonać to ćwiczenie
        /// </summary>
        [Display(Name = "Opis")]
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Identyfikator kategorii - klucz obcy do ExerciseCategory
        /// </summary>
        [ForeignKey(nameof(ExerciseCategory))]
        [Required]
        [Display(Name = "Kategoria")]
        public int ExerciseCategoryId { get; set; }

        /// <summary>
        /// Grupa mięśni (np. "Klatka", "Plecy", "Nogi")
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Grupa mięśni")]
        public string? MuscleGroup { get; set; }

        /// <summary>
        /// Poziom trudności ćwiczenia
        /// </summary>
        [Required]
        [Display(Name = "Trudność")]
        public ExerciseDifficulty Difficulty { get; set; } = ExerciseDifficulty.Medium;

        /// <summary>
        /// Data dodania ćwiczenia do systemu
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ========== RELACJE (NAWIGACJA) ==========

        /// <summary>
        /// Kategoria do której należy to ćwiczenie (relacja 1:1 zwrotna)
        /// VIRTUAL - umożliwia EF Core lazy loading
        /// </summary>
        public virtual ExerciseCategory? ExerciseCategory { get; set; }

        /// <summary>
        /// Ćwiczenie może być w wielu treningach (N:M przez WorkoutExercise)
        /// </summary>
        public virtual ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();

        /// <summary>
        /// Wpisy postępu dla tego ćwiczenia
        /// </summary>
        public virtual ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
    }

    /// <summary>
    /// Enum dla poziomu trudności ćwiczenia
    /// </summary>
    public enum ExerciseDifficulty
    {
        /// <summary>Ćwiczenie łatwe - dla początkujących</summary>
        Easy = 0,
        /// <summary>Ćwiczenie średnie - dla pośrednio zaawansowanych</summary>
        Medium = 1,
        /// <summary>Ćwiczenie trudne - dla zaawansowanych</summary>
        Hard = 2
    }
}