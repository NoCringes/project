using System;

namespace VolunteerClient.Models;

// Запросы и ответы аутентификации
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "volunteer";
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

// Мероприятия
public class EventDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int MaxVolunteers { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;  // ← ДОБАВИТЬ
    public List<EventSlotDto> Slots { get; set; } = new();
}

public class EventSlotDto
{
    public int SlotId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SlotsAvailable { get; set; }
    public int CurrentRegistrations { get; set; }
    public int FreeSlots => SlotsAvailable - CurrentRegistrations;
    public bool IsUserRegistered { get; set; } = false;
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
    public int SlotsAvailable { get; set; }
}

// Записи
public class SlotVolunteerDto
{
    public int RecordId { get; set; }
    public int UserId { get; set; }
    public int SlotId { get; set; }
    public int EventId { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? AttendedAt { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string SlotTitle { get; set; } = string.Empty;
    public string EventLocation { get; set; } = string.Empty;
    public DateTime EventStartDateTime { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public string VolunteerEmail { get; set; } = string.Empty;
    public string VolunteerPhone { get; set; } = string.Empty;
}
public class ApplyToSlotRequest
{
    public int SlotId { get; set; }
}

// Админские модели
public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}