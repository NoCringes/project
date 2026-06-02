using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerServer.Data;
using VolunteerServer.Models;

namespace VolunteerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ApplicationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/applications/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var registrations = await _context.SlotVolunteers
            .Where(sv => sv.UserId == userId)
            .ToListAsync();

        var result = new List<object>();

        foreach (var reg in registrations)
        {
            var slot = await _context.EventSlots.FindAsync(reg.SlotId);
            if (slot == null) continue;

            var eventEntity = await _context.Events.FindAsync(slot.EventId);
            if (eventEntity == null) continue;

            result.Add(new
            {
                reg.RecordId,
                reg.UserId,
                reg.SlotId,
                reg.RegisteredAt,
                reg.Status,
                reg.AttendedAt,
                EventId = eventEntity.EventId,
                EventTitle = eventEntity.Title,
                SlotTitle = slot.Title,
                EventLocation = eventEntity.Location,
                EventStartDateTime = eventEntity.StartDateTime
            });
        }

        return Ok(result);
    }

    // POST: api/applications/apply
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyToSlot([FromBody] ApplyToSlotRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var slot = await _context.EventSlots.FindAsync(request.SlotId);
        if (slot == null)
            return NotFound("Слот не найден");

        var currentRegistrations = await _context.SlotVolunteers
            .CountAsync(sv => sv.SlotId == request.SlotId && sv.Status == "registered");

        if (currentRegistrations >= slot.SlotsAvailable)
            return BadRequest("Нет свободных мест на этом слоте");

        var existing = await _context.SlotVolunteers
            .FirstOrDefaultAsync(sv => sv.UserId == userId && sv.SlotId == request.SlotId && sv.Status == "registered");

        if (existing != null)
            return BadRequest("Вы уже записаны на этот слот");

        var registration = new SlotVolunteer
        {
            UserId = userId,
            SlotId = request.SlotId,
            Status = "registered",
            RegisteredAt = DateTime.UtcNow
        };

        _context.SlotVolunteers.Add(registration);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Вы успешно записаны на слот", recordId = registration.RecordId });
    }

    // POST: api/applications/{id}/cancel
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var registration = await _context.SlotVolunteers.FindAsync(id);
        if (registration == null)
            return NotFound("Запись не найдена");

        if (registration.UserId != userId)
            return Forbid();

        if (registration.Status != "registered")
            return BadRequest("Можно отменить только активную запись");

        registration.Status = "cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Запись отменена" });
    }

    // POST: api/applications/{id}/attend
    [HttpPost("{id}/attend")]
    public async Task<IActionResult> MarkAttendance(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var registration = await _context.SlotVolunteers.FindAsync(id);
        if (registration == null)
            return NotFound("Запись не найдена");

        if (registration.UserId != userId)
            return Forbid();

        if (registration.Status != "registered")
            return BadRequest("Только активные записи можно отметить");

        registration.Status = "attended";
        registration.AttendedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Участие отмечено!" });
    }

    // POST: api/applications/{id}/confirm - подтвердить участие (для координатора)
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmAttendance(int id)
    {
        // Получаем userId из токена
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        var registration = await _context.SlotVolunteers.FindAsync(id);
        if (registration == null)
            return NotFound(new { message = "Запись не найдена" });

        // Проверяем, что пользователь - координатор или админ
        if (user.Role != "coordinator" && user.Role != "admin")
            return Forbid();

        // Проверяем, что координатор имеет доступ к этому мероприятию
        var slot = await _context.EventSlots.FindAsync(registration.SlotId);
        if (slot == null)
            return NotFound(new { message = "Слот не найден" });

        var eventEntity = await _context.Events.FindAsync(slot.EventId);
        if (eventEntity == null)
            return NotFound(new { message = "Мероприятие не найдено" });

        if (user.Role != "admin" && eventEntity.CreatedBy != userId)
            return Forbid();

        if (registration.Status != "registered")
            return BadRequest(new { message = "Можно подтвердить только активную запись" });

        registration.Status = "attended";
        registration.AttendedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Участие подтверждено!" });
    }
}

public class ApplyToSlotRequest
{
    public int SlotId { get; set; }
}