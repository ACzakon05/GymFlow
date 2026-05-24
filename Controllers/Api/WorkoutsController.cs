using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class WorkoutsController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public WorkoutsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Workout>>> GetWorkouts()
        {
            var workouts = await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == ApiUserId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return Ok(workouts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Workout>> GetWorkout(int id)
        {
            var workout = await _context.Workouts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == ApiUserId);

            if (workout == null) return NotFound(new { error = "Workout not found." });
            return Ok(workout);
        }

        [HttpPost]
        public async Task<ActionResult<Workout>> CreateWorkout(Workout workout)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            workout.UserId = ApiUserId;
            workout.CreatedAt = DateTime.UtcNow;

            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkout(int id, Workout workout)
        {
            if (id != workout.Id) return BadRequest(new { error = "Workout ID does not match route." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == ApiUserId);
            if (existing == null) return NotFound(new { error = "Workout not found." });

            existing.Name = workout.Name;
            existing.Description = workout.Description;
            existing.IsActive = workout.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Workouts.Update(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var existing = await _context.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.UserId == ApiUserId);
            if (existing == null) return NotFound(new { error = "Workout not found." });

            _context.Workouts.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
