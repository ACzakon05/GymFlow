using GymFlow.Data;
using GymFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymFlow.Controllers.Api
{
    [Route("api/[controller]")]
    public class UsersController : ApiBaseController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == ApiUserId);

            if (user == null) return NotFound(new { error = "User not found." });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                Role = user.Role.ToString(),
                user.ApiKey,
                user.CreatedAt,
                user.IsActive
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == ApiUserId);
            if (user == null) return NotFound(new { error = "User not found." });

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != user.Id))
                {
                    return BadRequest(new { error = "Email is already in use." });
                }
                user.Email = model.Email;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class UserUpdateModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }
    }
}
