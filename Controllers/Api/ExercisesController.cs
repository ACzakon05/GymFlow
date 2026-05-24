using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class ExercisesController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public ExercisesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
            var exercises = await _context.Exercises
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Ok(exercises);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Exercise>> GetExercise(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return NotFound(new { error = "Exercise not found." });
            return Ok(exercise);
        }

        [HttpPost]
        public async Task<ActionResult<Exercise>> CreateExercise(Exercise exercise)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _context.ExerciseCategories.FindAsync(exercise.ExerciseCategoryId);
            if (category == null) return BadRequest(new { error = "Exercise category not found." });

            exercise.CreatedAt = DateTime.UtcNow;
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(int id, Exercise exercise)
        {
            if (id != exercise.Id) return BadRequest(new { error = "Exercise ID does not match route." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.Exercises.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Exercise not found." });

            var category = await _context.ExerciseCategories.FindAsync(exercise.ExerciseCategoryId);
            if (category == null) return BadRequest(new { error = "Exercise category not found." });

            existing.Name = exercise.Name;
            existing.Description = exercise.Description;
            existing.ExerciseCategoryId = exercise.ExerciseCategoryId;
            existing.MuscleGroup = exercise.MuscleGroup;
            existing.Difficulty = exercise.Difficulty;

            _context.Update(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var existing = await _context.Exercises.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Exercise not found." });

            _context.Exercises.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
