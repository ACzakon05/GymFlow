using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Controllers
{
    /// <summary>
    /// Kontroler zarządzania ćwiczeniami
    /// </summary>
    [Authorize]
    public class ExercisesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExercisesController(ApplicationDbContext context)
    {
        _context=context;
    }

    public async Task<IActionResult> Index()
    {
        var exercises= await _context.Exercises.Include(e=>e.ExerciseCategory).ToListAsync();
        return View(exercises);
    }
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var exercise = await _context.Exercises.Include(e => e.ExerciseCategory).FirstOrDefaultAsync(e => e.Id == id);

        if (exercise == null) return NotFound();
        return View(exercise);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _context.ExerciseCategories.ToListAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exercise exercise)
    {
        if (ModelState.IsValid)
        {
            _context.Add(exercise);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ćwiczenie zostało dodane!";
            return RedirectToAction(nameof(Index));
        }
        var categories = await _context.ExerciseCategories.ToListAsync();
        ViewBag.Categories = categories;
        return View(exercise);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    { 
        if (id == null) return NotFound();
        var exercise = await _context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        var categories = await _context.ExerciseCategories.ToListAsync();
        ViewBag.Categories = categories;
        return View(exercise);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exercise exercise)
    {
        if (id != exercise.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(exercise);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ćwiczenie zostało zaktualizowane!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExerciseExists(exercise.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var categories = await _context.ExerciseCategories.ToListAsync();
        ViewBag.Categories = categories;
        return View(exercise);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var exercise=await _context.Exercises.FindAsync(id);
        if (exercise!= null)
        {
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sprawdza czy ćwiczenie istnieje w bazie
    /// </summary>
    private bool ExerciseExists(int id)
    {
        return _context.Exercises.Any(e => e.Id == id);
    }
}

}
