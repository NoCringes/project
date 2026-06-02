using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VolunteerClient.Models;

namespace VolunteerClient.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // ==================== АУТЕНТИФИКАЦИЯ ====================

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new Exception("Empty response from server");
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new Exception("Empty response from server");
    }

    // ==================== МЕРОПРИЯТИЯ ====================

    public async Task<List<EventDto>> GetEventsAsync()
    {
        var response = await _httpClient.GetAsync("/api/Events");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<EventDto>>();
        return result ?? new List<EventDto>();
    }

    public async Task<EventDto> GetEventAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"/api/Events/{eventId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EventDto>();
        return result ?? throw new Exception($"Event {eventId} not found");
    }

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Events", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EventDto>();
        return result ?? throw new Exception("Failed to create event");
    }

    public async Task UpdateEventAsync(int eventId, CreateEventRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/Events/{eventId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        var response = await _httpClient.DeleteAsync($"/api/Events/{eventId}");
        response.EnsureSuccessStatusCode();
    }

    // ==================== ЗАПИСИ ====================

    public async Task<List<SlotVolunteerDto>> GetMyApplicationsAsync()
    {
        var response = await _httpClient.GetAsync("/api/Applications/my");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SlotVolunteerDto>>();
        return result ?? new List<SlotVolunteerDto>();
    }

    public async Task ApplyToSlotAsync(int slotId)
    {
        var request = new ApplyToSlotRequest { SlotId = slotId };
        var response = await _httpClient.PostAsJsonAsync("/api/Applications/apply", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelApplicationAsync(int recordId)
    {
        var response = await _httpClient.PostAsync($"/api/Applications/{recordId}/cancel", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAttendanceAsync(int recordId)
    {
        var response = await _httpClient.PostAsync($"/api/Applications/{recordId}/attend", null);
        response.EnsureSuccessStatusCode();
    }

    // ==================== АДМИН ====================

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("/api/Admin/users");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        return result ?? new List<UserDto>();
    }

    public async Task UpdateUserAsync(int userId, UserDto user)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/Admin/users/{userId}", user);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var response = await _httpClient.DeleteAsync($"/api/Admin/users/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<object> GetStatsAsync()
    {
        var response = await _httpClient.GetAsync("/api/Admin/stats");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<object>();
        return result ?? new object();
    }
    public async Task ConfirmAttendanceAsync(int recordId)
    {
        var response = await _httpClient.PostAsync($"/api/Applications/{recordId}/confirm", null);
        response.EnsureSuccessStatusCode();
    }
    public async Task<List<SlotWithVolunteersDto>> GetEventRegistrationsAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"/api/Events/{eventId}/registrations");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SlotWithVolunteersDto>>();
        return result ?? new List<SlotWithVolunteersDto>();
    }
}