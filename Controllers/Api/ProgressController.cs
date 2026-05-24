using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class ProgressController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public ProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProgressEntry>>> GetProgressEntries()
        {
            var result = await _context.ProgressEntries
                .AsNoTracking()
                .Where(p => p.UserId == ApiUserId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProgressEntry>> GetProgressEntry(int id)
        {
            var entry = await _context.ProgressEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == ApiUserId);

            if (entry == null) return NotFound(new { error = "Progress entry not found." });
            return Ok(entry);
        }

        [HttpPost]
        public async Task<ActionResult<ProgressEntry>> CreateProgressEntry(ProgressEntry progressEntry)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exercise = await _context.Exercises.FindAsync(progressEntry.ExerciseId);
            if (exercise == null) return BadRequest(new { error = "Exercise not found." });

            progressEntry.UserId = ApiUserId;
            progressEntry.Date = DateTime.UtcNow;

            _context.ProgressEntries.Add(progressEntry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProgressEntry), new { id = progressEntry.Id }, progressEntry);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgressEntry(int id, ProgressEntry progressEntry)
        {
            if (id != progressEntry.Id) return BadRequest(new { error = "Progress entry ID does not match route." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.ProgressEntries.FirstOrDefaultAsync(p => p.Id == id && p.UserId == ApiUserId);
            if (existing == null) return NotFound(new { error = "Progress entry not found." });

            var exercise = await _context.Exercises.FindAsync(progressEntry.ExerciseId);
            if (exercise == null) return BadRequest(new { error = "Exercise not found." });

            existing.ExerciseId = progressEntry.ExerciseId;
            existing.Date = progressEntry.Date;
            existing.CompletedSets = progressEntry.CompletedSets;
            existing.CompletedReps = progressEntry.CompletedReps;
            existing.ActualWeight = progressEntry.ActualWeight;
            existing.IsCompleted = progressEntry.IsCompleted;
            existing.Notes = progressEntry.Notes;
            existing.Resistance = progressEntry.Resistance;

            _context.ProgressEntries.Update(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgressEntry(int id)
        {
            var existing = await _context.ProgressEntries.FirstOrDefaultAsync(p => p.Id == id && p.UserId == ApiUserId);
            if (existing == null) return NotFound(new { error = "Progress entry not found." });

            _context.ProgressEntries.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
