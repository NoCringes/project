using System;
using System.Collections.Generic;

namespace VolunteerClient.Models
{
    // ==================== АУТЕНТИФИКАЦИЯ ====================

    /// Запрос на вход (users: email, password_hash)
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// Запрос на регистрацию (users)
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }      // опционально
        public string AvatarUrl { get; set; } = string.Empty; // опционально
        public string Role { get; set; } = "volunteer";
    }

    /// Ответ сервера при аутентификации (users)
    public class AuthResponse
    {
        public int UserId { get; set; }          // user_id
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   // volunteer / coordinator
        public string Token { get; set; } = string.Empty;  // JWT токен (не в БД)
    }

    // ==================== МЕРОПРИЯТИЯ (events) ====================

    /// Мероприятие (соответствует таблице events)
    public class EventDto
    {
        public int EventId { get; set; }          // event_id (PK)
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;   // active / completed / cancelled
        public string Location { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }          // start_datetime
        public DateTime EndDateTime { get; set; }            // end_datetime
        public int MaxVolunteers { get; set; }               // max_volunteers
        public int CreatedBy { get; set; }                   // created_by (FK → users.user_id)
        public DateTime CreatedAt { get; set; }              // created_at

        // Не в БД, но удобно для клиента (загружается отдельно)
        public List<EventSlotDto> Slots { get; set; } = new();
    }

    /// Запрос на создание мероприятия
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

    // ==================== СЛОТЫ МЕРОПРИЯТИЙ (event_slots) ====================

    /// Слот мероприятия 
    public class EventSlotDto
    {
        public int SlotId { get; set; }            // slot_id (PK)
        public int EventId { get; set; }           // event_id (FK → events.event_id)
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SlotsAvailable { get; set; }    // slots_available

        // Вычисляемое поле (не в БД)
        public int CurrentRegistrations { get; set; }
        public int FreeSlots => SlotsAvailable - CurrentRegistrations;
    }

    /// <summary>
    /// Запрос на создание слота
    /// </summary>
    public class CreateSlotRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SlotsAvailable { get; set; }
    }

    // ==================== ЗАПИСИ ВОЛОНТЁРОВ (slot_volunteers) ====================

    /// <summary>
    /// Запись волонтёра на слот (соответствует таблице slot_volunteers)
    /// </summary>
    public class SlotVolunteerDto
    {
        public int RecordId { get; set; }          // record_id (PK)
        public int UserId { get; set; }            // user_id (FK → users.user_id)
        public int SlotId { get; set; }            // slot_id (FK → event_slots.slot_id)
        public DateTime RegisteredAt { get; set; } // registered_at
        public string Status { get; set; } = string.Empty;  // registered / cancelled / attended
        public DateTime? AttendedAt { get; set; }  // attended_at (NULL если не отметился)

        // Дополнительные поля для отображения (приходят с сервера через JOIN)
        public string VolunteerName { get; set; } = string.Empty;   // first_name + last_name
        public string VolunteerEmail { get; set; } = string.Empty;
        public string VolunteerPhone { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string SlotTitle { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public DateTime EventStartDateTime { get; set; }
    }

    /// <summary>
    /// Запрос на запись на слот
    /// </summary>
    public class ApplyToSlotRequest
    {
        public int UserId { get; set; }   // волонтёр
        public int SlotId { get; set; }   // слот, на который записывается
    }

    /// <summary>
    /// Запрос на обновление статуса записи
    /// </summary>
    public class UpdateRegistrationRequest
    {
        public int RecordId { get; set; }
        public string Status { get; set; } = string.Empty;  // cancelled / attended
    }
}