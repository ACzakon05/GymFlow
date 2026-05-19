using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymFlow.Controllers
{
    /// <summary>
    /// Kontroler do śledzenia postępu treningów
    /// </summary>
    [Authorize]
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(ApplicationDbContext context, ILogger<ProgressController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========== INDEX - LISTA WPISÓW POSTĘPU ==========

        /// <summary>
        /// GET - wyświetla listę wpisów postępu użytkownika
        /// </summary>
        public async Task<IActionResult> Index(int? exerciseId = null)
        {
            int userId = GetCurrentUserId();

            var query = (IOrderedQueryable<ProgressEntry>)_context.ProgressEntries
                .Where(p => p.UserId == userId)
                .Include(p => p.Exercise)
                .ThenInclude(e => e!.ExerciseCategory)
                .OrderByDescending(p => p.Date);

            // Filtracja po ćwiczeniu jeśli podane
            if (exerciseId.HasValue)
            {
                query = query.Where(p => p.ExerciseId == exerciseId.Value) as IOrderedQueryable<ProgressEntry> ?? throw new InvalidOperationException();
                ViewBag.ExerciseId = exerciseId;

                var exercise = await _context.Exercises.FindAsync(exerciseId);
                ViewBag.ExerciseName = exercise?.Name;
            }

            var progressEntries = await query.ToListAsync();

            return View(progressEntries);
        }

        // ========== DETAILS - SZCZEGÓŁY WPISU ==========

        /// <summary>
        /// GET - wyświetla szczegóły wpisu postępu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            int userId = GetCurrentUserId();

            var progressEntry = await _context.ProgressEntries
                .Where(p => p.Id == id && p.UserId == userId)
                .Include(p => p.Exercise)
                .ThenInclude(e => e.ExerciseCategory)
                .FirstOrDefaultAsync();

            if (progressEntry == null) return NotFound();

            return View(progressEntry);
        }

        // ========== CREATE - DODAJ WPIS POSTĘPU ==========

        /// <summary>
        /// GET - wyświetla formularz dodania wpisu postępu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int? exerciseId = null)
        {
            int userId = GetCurrentUserId();

            // Pobierz wszystkie ćwiczenia
            var exercises = await _context.Exercises
                .Include(e => e.ExerciseCategory)
                .OrderBy(e => e.Name)
                .ToListAsync();

            ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                exercises,
                nameof(Exercise.Id),
                nameof(Exercise.Name)
            );

            var progressEntry = new ProgressEntry
            {
                Date = DateTime.UtcNow,
                ExerciseId = exerciseId ?? 0,
                UserId = userId
            };

            return View(progressEntry);
        }

        /// <summary>
        /// POST - zapisuje nowy wpis postępu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgressEntry progressEntry)
        {
            int userId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                var exercises = await _context.Exercises
                    .Include(e => e.ExerciseCategory)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    exercises,
                    nameof(Exercise.Id),
                    nameof(Exercise.Name),
                    progressEntry.ExerciseId
                );
                return View(progressEntry);
            }

            // Sprawdzenie czy ćwiczenie istnieje
            var exercise = await _context.Exercises.FindAsync(progressEntry.ExerciseId);
            if (exercise == null)
            {
                ModelState.AddModelError("ExerciseId", "Wybrane ćwiczenie nie istnieje!");
                var exercises = await _context.Exercises
                    .Include(e => e.ExerciseCategory)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    exercises,
                    nameof(Exercise.Id),
                    nameof(Exercise.Name),
                    progressEntry.ExerciseId
                );
                return View(progressEntry);
            }

            progressEntry.UserId = userId;
            progressEntry.Date = DateTime.UtcNow;

            try
            {
                _context.Add(progressEntry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wpis postępu został zapisany!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy zapisywaniu wpisu postępu: {ex.Message}");
                ModelState.AddModelError("", "Błąd przy zapisywaniu wpisu!");
                
                var exercises = await _context.Exercises
                    .Include(e => e.ExerciseCategory)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    exercises,
                    nameof(Exercise.Id),
                    nameof(Exercise.Name),
                    progressEntry.ExerciseId
                );
                return View(progressEntry);
            }
        }

        // ========== EDIT - EDYTUJ WPIS ==========

        /// <summary>
        /// GET - wyświetla formularz edycji
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            int userId = GetCurrentUserId();

            var progressEntry = await _context.ProgressEntries
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (progressEntry == null) return NotFound();

            var exercises = await _context.Exercises
                .Include(e => e.ExerciseCategory)
                .OrderBy(e => e.Name)
                .ToListAsync();

            ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                exercises,
                nameof(Exercise.Id),
                nameof(Exercise.Name),
                progressEntry.ExerciseId
            );

            return View(progressEntry);
        }

        /// <summary>
        /// POST - zapisuje zmiany wpisu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProgressEntry progressEntry)
        {
            if (id != progressEntry.Id) return NotFound();

            int userId = GetCurrentUserId();

            var existingEntry = await _context.ProgressEntries
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (existingEntry == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var exercises = await _context.Exercises
                    .Include(e => e.ExerciseCategory)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                ViewBag.Exercises = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    exercises,
                    nameof(Exercise.Id),
                    nameof(Exercise.Name),
                    progressEntry.ExerciseId
                );
                return View(progressEntry);
            }

            try
            {
                existingEntry.ExerciseId = progressEntry.ExerciseId;
                existingEntry.CompletedSets = progressEntry.CompletedSets;
                existingEntry.CompletedReps = progressEntry.CompletedReps;
                existingEntry.ActualWeight = progressEntry.ActualWeight;
                existingEntry.IsCompleted = progressEntry.IsCompleted;
                existingEntry.Notes = progressEntry.Notes;
                existingEntry.Resistance = progressEntry.Resistance;

                _context.Update(existingEntry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wpis został zaktualizowany!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgressEntryExists(id))
                    return NotFound();
                throw;
            }
        }

        // ========== DELETE - USUŃ WPIS ==========

        /// <summary>
        /// POST - usuwa wpis postępu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetCurrentUserId();

            var progressEntry = await _context.ProgressEntries
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (progressEntry != null)
            {
                _context.ProgressEntries.Remove(progressEntry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wpis został usunięty!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== STATISTICS - STATYSTYKI ==========

        /// <summary>
        /// GET - wyświetla statystyki postępu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            int userId = GetCurrentUserId();

            var progressEntries = await _context.ProgressEntries
                .Where(p => p.UserId == userId)
                .Include(p => p.Exercise)
                .ThenInclude(e => e!.ExerciseCategory)
                .ToListAsync();

            var totalWorkouts = progressEntries.Count(p => p.IsCompleted);
            var totalExercises = progressEntries.Select(p => p.ExerciseId).Distinct().Count();
            var maxWeight = progressEntries.Where(p => p.ActualWeight > 0).Max(p => (decimal?)p.ActualWeight) ?? 0;
            var avgReps = progressEntries.Where(p => p.CompletedReps > 0).Count() > 0 
                ? progressEntries.Where(p => p.CompletedReps > 0).Average(p => p.CompletedReps) 
                : 0;

            // Grupuj po ćwiczeniach
            var exerciseStats = progressEntries
                .Where(p => p.Exercise != null)
                .GroupBy(p => p.Exercise)
                .Select(g => new ExerciseStatistic
                {
                    ExerciseId = g.Key!.Id,
                    ExerciseName = g.Key!.Name,
                    CategoryName = g.Key!.ExerciseCategory?.Name ?? "Bez Kategorii",
                    EntryCount = g.Count(),
                    MaxWeight = g.Where(p => p.ActualWeight > 0).Max(p => (decimal?)p.ActualWeight) ?? 0,
                    AvgReps = g.Where(p => p.CompletedReps > 0).Count() > 0 
                        ? g.Where(p => p.CompletedReps > 0).Average(p => p.CompletedReps) 
                        : 0
                })
                .OrderByDescending(s => s.EntryCount)
                .ToList();

            var statistics = new ProgressStatistics
            {
                TotalWorkouts = totalWorkouts,
                TotalExercises = totalExercises,
                MaxWeight = maxWeight,
                AvgReps = avgReps,
                ExerciseStats = exerciseStats
            };

            return View(statistics);
        }

        // ========== LOG WORKOUT - ZALOGUJ CAŁY TRENING ==========

        /// <summary>
        /// GET - wyświetla formularz do zalogowania całego treningu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LogWorkout(int? workoutId = null)
        {
            int userId = GetCurrentUserId();

            // Pobierz wszystkie treningi użytkownika
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            if (workoutId.HasValue)
            {
                // Jeśli wybrany konkretny trening, pobierz jego ćwiczenia
                var workout = await _context.Workouts
                    .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                    .ThenInclude(e => e!.ExerciseCategory)
                    .FirstOrDefaultAsync(w => w.Id == workoutId.Value && w.UserId == userId);

                if (workout == null)
                    return NotFound();

                ViewBag.Workout = workout;
                ViewBag.WorkoutExercises = workout.WorkoutExercises.OrderBy(we => we.Order).ToList();
                ViewBag.SelectedWorkoutId = workoutId;
            }

            ViewBag.Workouts = workouts;

            return View(new LogWorkoutViewModel { WorkoutId = workoutId ?? 0 });
        }

        /// <summary>
        /// POST - zapisuje postęp dla całego treningu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogWorkout(int workoutId, Dictionary<int, WorkoutExerciseLogData> exercisesData)
        {
            int userId = GetCurrentUserId();

            var workout = await _context.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == userId);

            if (workout == null)
                return NotFound();

            try
            {
                int savedCount = 0;

                foreach (var workoutExercise in workout.WorkoutExercises)
                {
                    if (!exercisesData.ContainsKey(workoutExercise.Id))
                        continue;

                    var logData = exercisesData[workoutExercise.Id];

                    // Utwórz wpis postępu dla każdego ćwiczenia
                    var progressEntry = new ProgressEntry
                    {
                        UserId = userId,
                        ExerciseId = workoutExercise.ExerciseId,
                        Date = DateTime.UtcNow,
                        CompletedSets = logData.Sets,
                        CompletedReps = logData.Reps,
                        ActualWeight = logData.Weight,
                        IsCompleted = logData.IsCompleted,
                        Notes = logData.Notes,
                        Resistance = logData.Resistance
                    };

                    _context.Add(progressEntry);
                    savedCount++;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Trening został zalogowany! Zapisano {savedCount} wpisów postępu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd przy zalogowaniu treningu: {ex.Message}");
                TempData["Error"] = "Błąd przy zalogowaniu treningu!";
                return RedirectToAction(nameof(LogWorkout), new { workoutId });
            }
        }

        // ========== HELPER METHODS ==========

        private bool ProgressEntryExists(int id)
        {
            return _context.ProgressEntries.Any(p => p.Id == id);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }
}
