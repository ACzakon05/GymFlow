using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymFlow.Controllers
{
    /// <summary>
    /// Kontroler zarządzania treningami
    /// </summary>
    [Authorize]
    public class WorkoutsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkoutsController> _logger;

        public WorkoutsController(ApplicationDbContext context, ILogger<WorkoutsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========== INDEX - LISTA TRENINGÓW ==========

        /// <summary>
        /// GET - wyświetla listę treningów użytkownika
        /// </summary>
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(workouts);
        }

        // ========== DETAILS - SZCZEGÓŁY TRENINGU ==========

        /// <summary>
        /// GET - wyświetla szczegóły treningu z listą ćwiczeń
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            int userId = GetCurrentUserId();

            var workout = await _context.Workouts
                .Where(w => w.Id == id && w.UserId == userId)
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .ThenInclude(e => e.ExerciseCategory)
                .FirstOrDefaultAsync();

            if (workout == null) return NotFound();

            return View(workout);
        }

        // ========== CREATE - NOWY TRENING ==========

        /// <summary>
        /// GET - wyświetla formularz tworzenia treningu
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST - zapisuje nowy trening
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workout workout)
        {
            if (!ModelState.IsValid)
            {
                return View(workout);
            }

            // Ustawienie użytkownika
            workout.UserId = GetCurrentUserId();
            workout.CreatedAt = DateTime.UtcNow;
            workout.IsActive = true;

            try
            {
                _context.Add(workout);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trening został utworzony!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy tworzeniu treningu: {ex.Message}");
                ModelState.AddModelError("", "Błąd przy tworzeniu treningu!");
                return View(workout);
            }
        }

        // ========== EDIT - EDYCJA TRENINGU ==========

        /// <summary>
        /// GET - wyświetla formularz edycji
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            int userId = GetCurrentUserId();

            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (workout == null) return NotFound();

            return View(workout);
        }

        /// <summary>
        /// POST - zapisuje zmiany treningu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Workout workout)
        {
            if (id != workout.Id) return NotFound();

            int userId = GetCurrentUserId();

            // Sprawdzenie czy trening należy do użytkownika
            var existingWorkout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (existingWorkout == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(workout);
            }

            try
            {
                existingWorkout.Name = workout.Name;
                existingWorkout.Description = workout.Description;
                existingWorkout.IsActive = workout.IsActive;
                existingWorkout.UpdatedAt = DateTime.UtcNow;

                _context.Update(existingWorkout);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trening został zaktualizowany!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkoutExists(id))
                    return NotFound();
                throw;
            }
        }

        // ========== DELETE - USUŃ TRENING ==========

        /// <summary>
        /// POST - usuwa trening
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetCurrentUserId();

            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

            if (workout != null)
            {
                _context.Workouts.Remove(workout);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Trening został usunięty!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== ADD EXERCISE - DODAJ ĆWICZENIE DO TRENINGU ==========

        /// <summary>
        /// GET - wyświetla listę dostępnych ćwiczeń do dodania
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddExercise(int? workoutId)
        {
            if (workoutId == null) return NotFound();

            int userId = GetCurrentUserId();

            // Sprawdzenie czy trening należy do użytkownika
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

            if (workout == null) return NotFound();

            // Pobierz ćwiczenia które już są w treningu
            var exercisesInWorkout = await _context.WorkoutExercises
                .Where(we => we.WorkoutId == workoutId)
                .Select(we => we.ExerciseId)
                .ToListAsync();

            // Pobierz wszystkie ćwiczenia oprócz tych w treningu
            var availableExercises = await _context.Exercises
                .Where(e => !exercisesInWorkout.Contains(e.Id))
                .Include(e => e.ExerciseCategory)
                .ToListAsync();

            ViewBag.WorkoutId = workoutId;
            ViewBag.WorkoutName = workout.Name;

            return View(availableExercises);
        }

        /// <summary>
        /// POST - dodaje ćwiczenie do treningu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExercise(int workoutId, int exerciseId, int sets, int reps)
        {
            int userId = GetCurrentUserId();

            // Sprawdzenie czy trening należy do użytkownika
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

            if (workout == null) return NotFound();

            // Sprawdzenie czy ćwiczenie już istnieje w treningu
            var existingExercise = await _context.WorkoutExercises
                .FirstOrDefaultAsync(we => we.WorkoutId == workoutId && we.ExerciseId == exerciseId);

            if (existingExercise != null)
            {
                TempData["Warning"] = "To ćwiczenie już jest w treningu!";
                return RedirectToAction(nameof(Details), new { id = workoutId });
            }

            // Pobierz ostatni order
            int maxOrder = await _context.WorkoutExercises
                .Where(we => we.WorkoutId == workoutId)
                .MaxAsync(we => (int?)we.Order) ?? 0;

            var workoutExercise = new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = exerciseId,
                Sets = sets > 0 ? sets : 3,
                Reps = reps > 0 ? reps : 10,
                Order = maxOrder + 1
            };

            try
            {
                _context.WorkoutExercises.Add(workoutExercise);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ćwiczenie dodane do treningu!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy dodawaniu ćwiczenia: {ex.Message}");
                TempData["Error"] = "Błąd przy dodawaniu ćwiczenia!";
            }

            return RedirectToAction(nameof(Details), new { id = workoutId });
        }

        // ========== REMOVE EXERCISE - USUŃ ĆWICZENIE Z TRENINGU ==========

        /// <summary>
        /// POST - usuwa ćwiczenie z treningu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveExercise(int workoutId, int workoutExerciseId)
        {
            int userId = GetCurrentUserId();

            // Sprawdzenie czy trening należy do użytkownika
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

            if (workout == null) return NotFound();

            var workoutExercise = await _context.WorkoutExercises
                .FirstOrDefaultAsync(we => we.Id == workoutExerciseId && we.WorkoutId == workoutId);

            if (workoutExercise != null)
            {
                _context.WorkoutExercises.Remove(workoutExercise);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ćwiczenie usunięte z treningu!";
            }

            return RedirectToAction(nameof(Details), new { id = workoutId });
        }

        // ========== HELPER METHODS ==========

        private bool WorkoutExists(int id)
        {
            return _context.Workouts.Any(w => w.Id == id);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }
}
