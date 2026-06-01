using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerServer.Data;
using VolunteerServer.Models;

namespace VolunteerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SlotsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SlotsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/slots/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlotById(int id)
    {
        var slot = await _context.EventSlots
            .Where(s => s.SlotId == id)
            .Select(s => new
            {
                s.SlotId,
                s.EventId,
                s.Title,
                s.Description,
                s.SlotsAvailable,
                CurrentRegistrations = _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered")
            })
            .FirstOrDefaultAsync();

        if (slot == null)
            return NotFound(new { message = "Слот не найден" });

        return Ok(slot);
    }

    // PUT: api/slots/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSlot(int id, [FromBody] UpdateSlotRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        var slot = await _context.EventSlots.FindAsync(id);
        if (slot == null)
            return NotFound(new { message = "Слот не найден" });

        var eventEntity = await _context.Events.FindAsync(slot.EventId);
        if (user.Role != "admin" && (user.Role != "coordinator" || eventEntity?.CreatedBy != userId))
            return StatusCode(403, new { message = "Нет прав на редактирование этого слота" });

        if (!string.IsNullOrEmpty(request.Title))
            slot.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            slot.Description = request.Description;

        if (request.SlotsAvailable.HasValue)
            slot.SlotsAvailable = request.SlotsAvailable.Value;

        await _context.SaveChangesAsync();

        var result = new
        {
            slot.SlotId,
            slot.EventId,
            slot.Title,
            slot.Description,
            slot.SlotsAvailable
        };

        return Ok(result);
    }

    // DELETE: api/slots/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        var slot = await _context.EventSlots.FindAsync(id);
        if (slot == null)
            return NotFound(new { message = "Слот не найден" });

        var eventEntity = await _context.Events.FindAsync(slot.EventId);
        if (user.Role != "admin" && (user.Role != "coordinator" || eventEntity?.CreatedBy != userId))
            return StatusCode(403, new { message = "Нет прав на удаление этого слота" });

        _context.EventSlots.Remove(slot);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Слот удалён" });
    }
}