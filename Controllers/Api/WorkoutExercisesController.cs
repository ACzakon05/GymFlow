using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class WorkoutExercisesController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public WorkoutExercisesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutExercise>>> GetWorkoutExercises(int? workoutId = null)
        {
            var query = _context.WorkoutExercises
                .AsNoTracking()
                .Where(we => we.Workout != null && we.Workout.UserId == ApiUserId);

            if (workoutId.HasValue)
            {
                query = query.Where(we => we.WorkoutId == workoutId.Value);
            }

            var result = await query.OrderBy(we => we.Order).ToListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkoutExercise>> GetWorkoutExercise(int id)
        {
            var entry = await _context.WorkoutExercises
                .AsNoTracking()
                .Include(we => we.Workout)
                .FirstOrDefaultAsync(we => we.Id == id && we.Workout != null && we.Workout.UserId == ApiUserId);

            if (entry == null) return NotFound(new { error = "Workout exercise not found." });
            return Ok(entry);
        }

        [HttpPost]
        public async Task<ActionResult<WorkoutExercise>> CreateWorkoutExercise(WorkoutExercise workoutExercise)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var workout = await _context.Workouts.FindAsync(workoutExercise.WorkoutId);
            if (workout == null || workout.UserId != ApiUserId)
            {
                return BadRequest(new { error = "Workout not found or does not belong to current user." });
            }

            var exercise = await _context.Exercises.FindAsync(workoutExercise.ExerciseId);
            if (exercise == null) return BadRequest(new { error = "Exercise not found." });

            _context.WorkoutExercises.Add(workoutExercise);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkoutExercise), new { id = workoutExercise.Id }, workoutExercise);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkoutExercise(int id, WorkoutExercise workoutExercise)
        {
            if (id != workoutExercise.Id) return BadRequest(new { error = "Workout exercise ID mismatch." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.WorkoutExercises
                .Include(we => we.Workout)
                .FirstOrDefaultAsync(we => we.Id == id && we.Workout != null && we.Workout.UserId == ApiUserId);

            if (existing == null) return NotFound(new { error = "Workout exercise not found." });

            var workout = await _context.Workouts.FindAsync(workoutExercise.WorkoutId);
            if (workout == null || workout.UserId != ApiUserId)
            {
                return BadRequest(new { error = "Workout not found or does not belong to current user." });
            }

            var exercise = await _context.Exercises.FindAsync(workoutExercise.ExerciseId);
            if (exercise == null) return BadRequest(new { error = "Exercise not found." });

            existing.WorkoutId = workoutExercise.WorkoutId;
            existing.ExerciseId = workoutExercise.ExerciseId;
            existing.Sets = workoutExercise.Sets;
            existing.Reps = workoutExercise.Reps;
            existing.Weight = workoutExercise.Weight;
            existing.RestSeconds = workoutExercise.RestSeconds;
            existing.Notes = workoutExercise.Notes;
            existing.Order = workoutExercise.Order;

            _context.WorkoutExercises.Update(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkoutExercise(int id)
        {
            var existing = await _context.WorkoutExercises
                .Include(we => we.Workout)
                .FirstOrDefaultAsync(we => we.Id == id && we.Workout != null && we.Workout.UserId == ApiUserId);

            if (existing == null) return NotFound(new { error = "Workout exercise not found." });

            _context.WorkoutExercises.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
