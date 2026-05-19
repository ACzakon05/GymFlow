using System.ComponentModel.DataAnnotations;

namespace GymFlow.Models
{
    /// <summary>
    /// Kategoria ćwiczenia - np. "Klatka piersiowa", "Plecy", "Nogi"
    /// </summary>
    public class ExerciseCategory
    {
        /// <summary>
        /// Unikalny identyfikator kategorii (klucz główny)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa kategorii - unikalna i wymagana
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Nazwa Kategorii")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Opcjonalny opis kategorii
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        /// <summary>
        /// Data utworzenia kategorii
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ćwiczenia należące do tej kategorii (1 kategoria : wiele ćwiczeń)
        /// VIRTUAL - umożliwia EF Core lazy loading i śledzenie zmian
        /// </summary>
        public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}