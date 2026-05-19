using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymFlow.Models
{
    /// <summary>
    /// Model treningu - zestaw ćwiczeń stworzony przez użytkownika
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// Unikalny identyfikator treningu (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa treningu - wymagana
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Nazwa treningu")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Opcjonalny opis treningu
        /// </summary>
        [StringLength(1000)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        /// <summary>
        /// Identyfikator użytkownika - klucz obcy do User
        /// </summary>
        [ForeignKey(nameof(User))]
        [Required]
        [Display(Name = "Użytkownik")]
        public int UserId { get; set; }

        /// <summary>
        /// Data utworzenia treningu
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ostatniej edycji treningu
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Czy trening jest aktywny (używany)
        /// </summary>
        [Display(Name = "Aktywny")]
        public bool IsActive { get; set; } = true;

        // ========== RELACJE (NAWIGACJA) ==========

        /// <summary>
        /// Użytkownik który stworzył ten trening (1:1 zwrotna)
        /// VIRTUAL - umożliwia EF Core lazy loading
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Ćwiczenia w tym treningu (N:M przez WorkoutExercise)
        /// </summary>
        public virtual ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
    }
}
