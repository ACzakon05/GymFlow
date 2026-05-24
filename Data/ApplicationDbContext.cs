using GymFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Data
{
    /// <summary>
    /// DbContext dla aplikacji GymFlow - główna klasa dostępu do bazy danych
    /// Odpowiada za mapowanie modeli C# na tabele w bazie SQLite
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Konstruktor - przyjmuje opcje konfiguracji DbContext
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        // ========== DbSet - reprezentują tabele w bazie danych ==========

        /// <summary>
        /// DbSet dla tabeli Users
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// DbSet dla tabeli ExerciseCategories
        /// </summary>
        public DbSet<ExerciseCategory> ExerciseCategories { get; set; }

        /// <summary>
        /// DbSet dla tabeli Exercises
        /// </summary>
        public DbSet<Exercise> Exercises { get; set; }

        /// <summary>
        /// DbSet dla tabeli Workouts
        /// </summary>
        public DbSet<Workout> Workouts { get; set; }

        /// <summary>
        /// DbSet dla tabeli WorkoutExercises (relacja N:M)
        /// </summary>
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

        /// <summary>
        /// DbSet dla tabeli ProgressEntries
        /// </summary>
        public DbSet<ProgressEntry> ProgressEntries { get; set; }

        /// <summary>
        /// Metoda OnModelCreating - konfiguracja modeli i relacji
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== KONFIGURACJA UŻYTKOWNIKA ==========

            modelBuilder.Entity<User>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Indeksy unikalne
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Właściwości
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

                // Relacje
                entity.HasMany(e => e.Workouts)
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ProgressEntries)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== KONFIGURACJA KATEGORII ĆWICZEŃ ==========

            modelBuilder.Entity<ExerciseCategory>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Indeks unikalny
                entity.HasIndex(e => e.Name).IsUnique();

                // Właściwości
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacje
                entity.HasMany(e => e.Exercises)
                    .WithOne(ex => ex.ExerciseCategory)
                    .HasForeignKey(ex => ex.ExerciseCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== KONFIGURACJA ĆWICZEŃ ==========

            modelBuilder.Entity<Exercise>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Właściwości
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ExerciseCategoryId).IsRequired();
                entity.Property(e => e.MuscleGroup).HasMaxLength(100);
                entity.Property(e => e.Difficulty).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacje
                entity.HasOne(e => e.ExerciseCategory)
                    .WithMany(ec => ec.Exercises)
                    .HasForeignKey(e => e.ExerciseCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.WorkoutExercises)
                    .WithOne(we => we.Exercise)
                    .HasForeignKey(we => we.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ProgressEntries)
                    .WithOne(p => p.Exercise)
                    .HasForeignKey(p => p.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== KONFIGURACJA TRENINGÓW ==========

            modelBuilder.Entity<Workout>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Właściwości
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

                // Relacje
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Workouts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.WorkoutExercises)
                    .WithOne(we => we.Workout)
                    .HasForeignKey(we => we.WorkoutId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== KONFIGURACJA RELACJI WORKOUT-EXERCISE (N:M) ==========

            modelBuilder.Entity<WorkoutExercise>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Właściwości
                entity.Property(e => e.WorkoutId).IsRequired();
                entity.Property(e => e.ExerciseId).IsRequired();
                entity.Property(e => e.Sets).IsRequired();
                entity.Property(e => e.Reps).IsRequired();
                entity.Property(e => e.Weight).HasPrecision(10, 2);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Order).IsRequired();

                // Relacje
                entity.HasOne(e => e.Workout)
                    .WithMany(w => w.WorkoutExercises)
                    .HasForeignKey(e => e.WorkoutId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Exercise)
                    .WithMany(ex => ex.WorkoutExercises)
                    .HasForeignKey(e => e.ExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indeks dla sortowania
                entity.HasIndex(e => new { e.WorkoutId, e.Order });
            });

            // ========== KONFIGURACJA WPISÓW POSTĘPU ==========

            modelBuilder.Entity<ProgressEntry>(entity =>
            {
                // Klucz główny
                entity.HasKey(e => e.Id);

                // Właściwości
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ExerciseId).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.CompletedSets).IsRequired();
                entity.Property(e => e.CompletedReps).IsRequired();
                entity.Property(e => e.ActualWeight).HasPrecision(10, 2);
                entity.Property(e => e.IsCompleted).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Resistance).HasMaxLength(50);

                // Relacje
                entity.HasOne(e => e.User)
                    .WithMany(u => u.ProgressEntries)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Exercise)
                    .WithMany(ex => ex.ProgressEntries)
                    .HasForeignKey(e => e.ExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indeks do szybkiego wyszukiwania postępu użytkownika
                entity.HasIndex(e => new { e.UserId, e.Date });
            });

            // ========== SEED DATA - WSTĘPNE DANE ==========

            // Dodaj domyślne kategorie ćwiczeń
            modelBuilder.Entity<ExerciseCategory>().HasData(
                new ExerciseCategory 
                { 
                    Id = 1, 
                    Name = "Siła", 
                    Description = "Ćwiczenia na budowanie siły mięśniowej",
                    CreatedAt = DateTime.UtcNow
                },
                new ExerciseCategory 
                { 
                    Id = 2, 
                    Name = "Kardio", 
                    Description = "Ćwiczenia na wytrzymałość sercowo-naczyniową",
                    CreatedAt = DateTime.UtcNow
                },
                new ExerciseCategory 
                { 
                    Id = 3, 
                    Name = "Elastyczność", 
                    Description = "Ćwiczenia na rozciąganie i elastyczność",
                    CreatedAt = DateTime.UtcNow
                },
                new ExerciseCategory 
                { 
                    Id = 4, 
                    Name = "Funkcjonalne", 
                    Description = "Ćwiczenia funkcjonalne dla całego ciała",
                    CreatedAt = DateTime.UtcNow
                },
                new ExerciseCategory 
                { 
                    Id = 5, 
                    Name = "Yoga", 
                    Description = "Ćwiczenia jogi",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Dodaj domyślnego admina
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 999,
                    Username = "admin",
                    Email = "admin@gymflow.pl",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    FirstName = "Administrator",
                    LastName = "GymFlow",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
