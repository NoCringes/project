using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using VolunteerClient.Models;

namespace VolunteerClient.Services
{
    /// Клиент для взаимодействия с сервером (ASP.NET Core API)
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private string? _authToken;

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                // ВАЖНО: Замените этот адрес на адрес вашего сервера!
                // Когда запустите сервер, он покажет адрес (обычно https://localhost:5001 или https://localhost:7000)
                BaseAddress = new Uri("https://localhost:5001/api/")
            };
        }

        /// Установить JWT токен для авторизации
        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// Очистить JWT токен (при выходе из системы)
        public void ClearAuthToken()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // ==================== АУТЕНТИФИКАЦИЯ ====================

        /// Вход в систему
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return result ?? throw new Exception("Empty response from server");
        }

        /// Регистрация нового пользователя
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return result ?? throw new Exception("Empty response from server");
        }

        // ==================== МЕРОПРИЯТИЯ ====================

        /// Получить список мероприятий (по умолчанию активные)
        public async Task<List<EventDto>> GetEventsAsync(string status = "active")
        {
            var response = await _httpClient.GetAsync($"events?status={status}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<EventDto>>();
            return result ?? new List<EventDto>();
        }

        /// Получить мероприятие по ID
        public async Task<EventDto> GetEventAsync(int eventId)
        {
            var response = await _httpClient.GetAsync($"events/{eventId}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EventDto>();
            return result ?? throw new Exception($"Event {eventId} not found");
        }

        /// <summary>
        /// Создать новое мероприятие (только для координатора)
        /// </summary>
        public async Task<EventDto> CreateEventAsync(CreateEventRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("events", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EventDto>();
            return result ?? throw new Exception("Failed to create event");
        }

        /// <summary>
        /// Удалить мероприятие (только для координатора)
        /// </summary>
        public async Task DeleteEventAsync(int eventId)
        {
            var response = await _httpClient.DeleteAsync($"events/{eventId}");
            response.EnsureSuccessStatusCode();
        }

        // ==================== ЗАПИСЬ НА СЛОТЫ ====================

        /// <summary>
        /// Получить все записи текущего волонтёра
        /// </summary>
        public async Task<List<SlotVolunteerDto>> GetMyRegistrationsAsync()
        {
            var response = await _httpClient.GetAsync("applications/my");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<SlotVolunteerDto>>();
            return result ?? new List<SlotVolunteerDto>();
        }

        /// <summary>
        /// Записаться на слот (волонтёр)
        /// </summary>
        public async Task ApplyToSlotAsync(int slotId, int userId)
        {
            var request = new ApplyToSlotRequest { SlotId = slotId, UserId = userId };
            var response = await _httpClient.PostAsJsonAsync("applications/apply", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Отменить запись на слот (волонтёр)
        /// </summary>
        public async Task CancelRegistrationAsync(int recordId)
        {
            var response = await _httpClient.PostAsync($"applications/{recordId}/cancel", null);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Отметить своё участие (волонтёр нажимает "Я пришёл")
        /// </summary>
        public async Task MarkAttendanceAsync(int recordId)
        {
            var response = await _httpClient.PostAsync($"applications/{recordId}/attend", null);
            response.EnsureSuccessStatusCode();
        }

        // ==================== ДЛЯ КООРДИНАТОРА ====================

        /// <summary>
        /// Получить список волонтёров, записавшихся на мероприятие
        /// </summary>
        public async Task<List<SlotVolunteerDto>> GetEventRegistrationsAsync(int eventId)
        {
            var response = await _httpClient.GetAsync($"events/{eventId}/registrations");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<SlotVolunteerDto>>();
            return result ?? new List<SlotVolunteerDto>();
        }
    }
}