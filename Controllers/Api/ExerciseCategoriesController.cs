using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class ExerciseCategoriesController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public ExerciseCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciseCategory>>> GetCategories()
        {
            var categories = await _context.ExerciseCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseCategory>> GetCategory(int id)
        {
            var category = await _context.ExerciseCategories.FindAsync(id);
            if (category == null) return NotFound(new { error = "Category not found." });
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<ExerciseCategory>> CreateCategory(ExerciseCategory category)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.ExerciseCategories.AnyAsync(c => c.Name == category.Name))
            {
                return BadRequest(new { error = "Category name already exists." });
            }

            category.CreatedAt = DateTime.UtcNow;
            _context.ExerciseCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, ExerciseCategory category)
        {
            if (id != category.Id) return BadRequest(new { error = "Category ID mismatch." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.ExerciseCategories.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Category not found." });

            if (await _context.ExerciseCategories.AnyAsync(c => c.Name == category.Name && c.Id != id))
            {
                return BadRequest(new { error = "Category name already exists." });
            }

            existing.Name = category.Name;
            existing.Description = category.Description;

            _context.ExerciseCategories.Update(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existing = await _context.ExerciseCategories.FindAsync(id);
            if (existing == null) return NotFound(new { error = "Category not found." });

            _context.ExerciseCategories.Remove(existing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
