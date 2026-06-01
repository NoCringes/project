using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerServer.Data;
using VolunteerServer.Models;

namespace VolunteerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/admin/users - получить всех пользователей
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Phone,
                u.DateOfBirth,
                u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/admin/users/{id} - получить пользователя по ID
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users
            .Where(u => u.UserId == id)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Phone,
                u.DateOfBirth,
                u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }

    // PUT: api/admin/users/{id} - обновить пользователя
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.Phone))
            user.Phone = request.Phone;

        if (!string.IsNullOrEmpty(request.Role) && request.Role != user.Role)
        {
            if (request.Role != "volunteer" && request.Role != "coordinator" && request.Role != "admin")
                return BadRequest(new { message = "Недопустимая роль" });

            if (user.Role == "admin" && request.Role != "admin")
                return BadRequest(new { message = "Нельзя изменить роль администратора" });

            user.Role = request.Role;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь обновлён" });
    }

    // DELETE: api/admin/users/{id} - удалить пользователя
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        if (user.Role == "admin")
            return BadRequest(new { message = "Нельзя удалить администратора" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь удалён" });
    }

    // GET: api/admin/events - получить все мероприятия
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events
            .Select(e => new
            {
                e.EventId,
                e.Title,
                e.Description,
                e.Location,
                e.StartDateTime,
                e.EndDateTime,
                e.MaxVolunteers,
                e.Status,
                e.CreatedBy,
                e.CreatedAt
            })
            .ToListAsync();

        return Ok(events);
    }

    // DELETE: api/admin/events/{id} - удалить любое мероприятие
    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteAnyEvent(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return NotFound(new { message = "Мероприятие не найдено" });

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Мероприятие удалено" });
    }

    // GET: api/admin/stats - статистика системы
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalVolunteers = await _context.Users.CountAsync(u => u.Role == "volunteer");
        var totalCoordinators = await _context.Users.CountAsync(u => u.Role == "coordinator");
        var totalEvents = await _context.Events.CountAsync();
        var totalRegistrations = await _context.SlotVolunteers.CountAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalVolunteers = totalVolunteers,
            TotalCoordinators = totalCoordinators,
            TotalEvents = totalEvents,
            TotalRegistrations = totalRegistrations
        });
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
}