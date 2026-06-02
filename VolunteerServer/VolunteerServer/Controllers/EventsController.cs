using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerServer.Data;
using VolunteerServer.Models;

namespace VolunteerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events
            .Where(e => e.Status == "active")
            .ToListAsync();

        var result = new List<object>();

        foreach (var e in events)
        {
            var coordinator = await _context.Users.FindAsync(e.CreatedBy);
            var creatorName = coordinator != null ? $"{coordinator.FirstName} {coordinator.LastName}" : "Неизвестно";

            var slots = await _context.EventSlots
                .Where(s => s.EventId == e.EventId)
                .Select(s => new
                {
                    s.SlotId,
                    s.Title,
                    s.Description,
                    s.SlotsAvailable,
                    CurrentRegistrations = _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered")
                })
                .ToListAsync();

            result.Add(new
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
                CreatorName = creatorName,
                Slots = slots
            });
        }

        return Ok(result);
    }

    // GET: api/events/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventById(int id)
    {
        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound($"Мероприятие с ID {id} не найдено");

        var slots = await _context.EventSlots
            .Where(s => s.EventId == id)
            .Select(s => new
            {
                s.SlotId,
                s.Title,
                s.Description,
                s.SlotsAvailable,
                CurrentRegistrations = _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered")
            })
            .ToListAsync();

        var result = new
        {
            eventItem.EventId,
            eventItem.Title,
            eventItem.Description,
            eventItem.Location,
            eventItem.StartDateTime,
            eventItem.EndDateTime,
            eventItem.MaxVolunteers,
            eventItem.Status,
            eventItem.CreatedBy,  // ← ДОБАВИТЬ ЭТУ СТРОКУ
            Slots = slots
        };

        return Ok(result);
    }

    // POST: api/events
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        if (user.Role != "coordinator" && user.Role != "admin")
            return StatusCode(403, new { message = "Только координаторы могут создавать мероприятия" });

        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            MaxVolunteers = request.MaxVolunteers,
            Status = "active",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        if (request.Slots != null)
        {
            foreach (var slotRequest in request.Slots)
            {
                var slot = new EventSlot
                {
                    EventId = eventEntity.EventId,
                    Title = slotRequest.Title,
                    Description = slotRequest.Description,
                    SlotsAvailable = slotRequest.SlotsAvailable
                };
                _context.EventSlots.Add(slot);
            }
            await _context.SaveChangesAsync();
        }

        var slotsResult = await _context.EventSlots
            .Where(s => s.EventId == eventEntity.EventId)
            .Select(s => new
            {
                s.SlotId,
                s.Title,
                s.Description,
                s.SlotsAvailable
            })
            .ToListAsync();

        var result = new
        {
            eventEntity.EventId,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.Status,
            eventEntity.Location,
            eventEntity.StartDateTime,
            eventEntity.EndDateTime,
            eventEntity.MaxVolunteers,
            eventEntity.CreatedBy,
            eventEntity.CreatedAt,
            Slots = slotsResult
        };

        return CreatedAtAction(nameof(GetEventById), new { id = eventEntity.EventId }, result);
    }

    // DELETE: api/events/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return NotFound(new { message = "Мероприятие не найдено" });

        if (user.Role != "admin" && (user.Role != "coordinator" || eventEntity.CreatedBy != userId))
            return StatusCode(403, new { message = "Нет прав на удаление этого мероприятия" });

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Мероприятие удалено" });
    }
    // GET: api/events/{id}/slots - получить все слоты мероприятия
    [HttpGet("{id}/slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventSlots(int id)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.EventId == id);
        if (!eventExists)
            return NotFound(new { message = "Мероприятие не найдено" });

        var slots = await _context.EventSlots
            .Where(s => s.EventId == id)
            .Select(s => new
            {
                s.SlotId,
                s.Title,
                s.Description,
                s.SlotsAvailable,
                CurrentRegistrations = _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered"),
                FreeSlots = s.SlotsAvailable - _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered")
            })
            .ToListAsync();

        return Ok(slots);
    }

    // GET: api/events/{id}/registrations - получить все слоты с волонтёрами
    [HttpGet("{id}/registrations")]
    public async Task<IActionResult> GetEventRegistrations(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return NotFound(new { message = "Мероприятие не найдено" });

        if (user.Role != "admin" && (user.Role != "coordinator" || eventEntity.CreatedBy != userId))
            return StatusCode(403, new { message = "Нет прав на просмотр записей этого мероприятия" });

        // Получаем все слоты мероприятия
        var slots = await _context.EventSlots
            .Where(s => s.EventId == id)
            .ToListAsync();

        var result = new List<object>();

        foreach (var slot in slots)
        {
            // Получаем волонтёров, записанных на этот слот
            var volunteers = await _context.SlotVolunteers
                .Where(sv => sv.SlotId == slot.SlotId)
                .Select(sv => new
                {
                    sv.RecordId,
                    sv.UserId,
                    sv.RegisteredAt,
                    sv.Status,
                    sv.AttendedAt,
                    VolunteerName = _context.Users.Where(u => u.UserId == sv.UserId).Select(u => $"{u.FirstName} {u.LastName}").FirstOrDefault() ?? "Неизвестно",
                    VolunteerEmail = _context.Users.Where(u => u.UserId == sv.UserId).Select(u => u.Email).FirstOrDefault() ?? "",
                    VolunteerPhone = _context.Users.Where(u => u.UserId == sv.UserId).Select(u => u.Phone).FirstOrDefault() ?? ""
                })
                .ToListAsync();

            result.Add(new
            {
                slot.SlotId,
                slot.Title,
                slot.Description,
                slot.SlotsAvailable,
                CurrentRegistrations = volunteers.Count(v => v.Status == "registered"),
                Volunteers = volunteers
            });
        }

        return Ok(result);
    }

    // PUT: api/events/{id} - обновить мероприятие
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        // Получаем userId из токена
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "Не авторизован" });

        var userId = int.Parse(userIdClaim);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Пользователь не найден" });

        // Находим мероприятие
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
            return NotFound(new { message = "Мероприятие не найдено" });

        // Проверка прав: координатор может редактировать только свои, админ - любые
        if (user.Role != "admin" && (user.Role != "coordinator" || eventEntity.CreatedBy != userId))
            return StatusCode(403, new { message = "Нет прав на редактирование этого мероприятия" });

        // Обновляем поля (только те, что пришли не null)
        if (!string.IsNullOrEmpty(request.Title))
            eventEntity.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            eventEntity.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Location))
            eventEntity.Location = request.Location;

        if (request.StartDateTime.HasValue)
            eventEntity.StartDateTime = request.StartDateTime.Value;

        if (request.EndDateTime.HasValue)
            eventEntity.EndDateTime = request.EndDateTime.Value;

        if (request.MaxVolunteers.HasValue)
            eventEntity.MaxVolunteers = request.MaxVolunteers.Value;

        await _context.SaveChangesAsync();

        // Обновляем слоты, если переданы

        if (request.Slots != null && request.Slots.Any())
        {
            // Удаляем старые слоты
            var oldSlots = await _context.EventSlots.Where(s => s.EventId == id).ToListAsync();
            _context.EventSlots.RemoveRange(oldSlots);

            // Добавляем новые слоты
            foreach (var slotRequest in request.Slots)
            {
                var slot = new EventSlot
                {
                    EventId = eventEntity.EventId,
                    Title = slotRequest.Title,
                    Description = slotRequest.Description,
                    SlotsAvailable = slotRequest.SlotsAvailable ?? 0  // ← ИСПРАВЛЕНО
                };
                _context.EventSlots.Add(slot);
            }
            await _context.SaveChangesAsync();
        }

        // Формируем ответ
        var slotsResult = await _context.EventSlots
            .Where(s => s.EventId == eventEntity.EventId)
            .Select(s => new
            {
                s.SlotId,
                s.Title,
                s.Description,
                s.SlotsAvailable,
                CurrentRegistrations = _context.SlotVolunteers.Count(sv => sv.SlotId == s.SlotId && sv.Status == "registered")
            })
            .ToListAsync();

        var result = new
        {
            eventEntity.EventId,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.Status,
            eventEntity.Location,
            eventEntity.StartDateTime,
            eventEntity.EndDateTime,
            eventEntity.MaxVolunteers,
            eventEntity.CreatedBy,
            eventEntity.CreatedAt,
            Slots = slotsResult
        };

        return Ok(result);
    }
}

public class UpdateEventRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public int? MaxVolunteers { get; set; }
    public List<UpdateSlotRequest>? Slots { get; set; }
}

public class UpdateSlotRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? SlotsAvailable { get; set; }  // ← int? (может быть null)
}

public class CreateEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int MaxVolunteers { get; set; }
    public List<CreateSlotRequest> Slots { get; set; } = new();
}

public class CreateSlotRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SlotsAvailable { get; set; }  // ← int (не может быть null)
}