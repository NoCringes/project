using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class AdminDashboardViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private ObservableCollection<UserDto> _users = new();
    private ObservableCollection<EventDto> _allEvents = new();
    private UserDto? _selectedUser;
    private EventDto? _selectedEvent;
    private string _searchText = string.Empty;
    private int _totalUsers;
    private int _totalVolunteers;
    private int _totalCoordinators;
    private int _totalEvents;

    public AdminDashboardViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Административная панель";

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        LogoutCommand = new RelayCommand(_ => Logout());

        EditUserCommand = new RelayCommand<UserDto?>(async user => await EditUserAsync(user));
        ChangeRoleCommand = new RelayCommand<UserDto?>(async user => await ChangeRoleAsync(user));
        DeleteUserCommand = new RelayCommand<UserDto?>(async user => await DeleteUserAsync(user));

        EditEventCommand = new RelayCommand<EventDto?>(async eventItem => await EditEventAsync(eventItem));
        DeleteEventCommand = new RelayCommand<EventDto?>(async eventItem => await DeleteEventAsync(eventItem));
        ViewVolunteersCommand = new RelayCommand<EventDto?>(async eventItem => await ViewVolunteersAsync(eventItem));

        Task.Run(async () => await LoadDataAsync());
    }

    public ObservableCollection<UserDto> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public ObservableCollection<EventDto> AllEvents
    {
        get => _allEvents;
        set => SetProperty(ref _allEvents, value);
    }

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public EventDto? SelectedEvent
    {
        get => _selectedEvent;
        set => SetProperty(ref _selectedEvent, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterUsers();
        }
    }

    public int TotalUsers
    {
        get => _totalUsers;
        set => SetProperty(ref _totalUsers, value);
    }

    public int TotalVolunteers
    {
        get => _totalVolunteers;
        set => SetProperty(ref _totalVolunteers, value);
    }

    public int TotalCoordinators
    {
        get => _totalCoordinators;
        set => SetProperty(ref _totalCoordinators, value);
    }

    public int TotalEvents
    {
        get => _totalEvents;
        set => SetProperty(ref _totalEvents, value);
    }

    public RelayCommand LoadDataCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand<UserDto?> EditUserCommand { get; }
    public RelayCommand<UserDto?> ChangeRoleCommand { get; }
    public RelayCommand<UserDto?> DeleteUserCommand { get; }
    public RelayCommand<EventDto?> EditEventCommand { get; }
    public RelayCommand<EventDto?> DeleteEventCommand { get; }
    public RelayCommand<EventDto?> ViewVolunteersCommand { get; }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var users = await _apiClient.GetAllUsersAsync();
            Users = new ObservableCollection<UserDto>(users);

            var events = await _apiClient.GetEventsAsync();
            AllEvents = new ObservableCollection<EventDto>(events);

            UpdateStatistics(users, events);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки данных: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateStatistics(List<UserDto> users, List<EventDto> events)
    {
        TotalUsers = users.Count;
        TotalVolunteers = users.Count(u => u.Role == "volunteer");
        TotalCoordinators = users.Count(u => u.Role == "coordinator");
        TotalEvents = events.Count;
    }

    private void FilterUsers()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Task.Run(async () => await LoadDataAsync());
            return;
        }

        var filtered = _users.Where(u =>
            u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            u.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            u.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Users = new ObservableCollection<UserDto>(filtered);
    }

    private async Task EditUserAsync(UserDto? user)
    {
        if (user is null) return;

        var dialog = new Views.EditUserDialog(user);
        dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() == true)
        {
            await _apiClient.UpdateUserAsync(user.UserId, user);
            await LoadDataAsync();
            _navigation.ShowMessage("Данные пользователя обновлены");
        }
    }

    private async Task ChangeRoleAsync(UserDto? user)
    {
        if (user is null) return;

        if (user.Role == "admin")
        {
            _navigation.ShowError("Нельзя изменить роль администратора");
            return;
        }

        var newRole = user.Role == "volunteer" ? "coordinator" : "volunteer";

        var result = MessageBox.Show($"Сменить роль пользователя {user.FirstName} {user.LastName} с '{user.Role}' на '{newRole}'?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;

        try
        {
            user.Role = newRole;
            await _apiClient.UpdateUserAsync(user.UserId, user);
            _navigation.ShowMessage("Роль успешно изменена");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка изменения роли: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteUserAsync(UserDto? user)
    {
        if (user is null) return;

        if (user.Role == "admin")
        {
            _navigation.ShowError("Нельзя удалить администратора");
            return;
        }

        var result = MessageBox.Show($"Удалить пользователя {user.FirstName} {user.LastName}?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;

        try
        {
            await _apiClient.DeleteUserAsync(user.UserId);
            _navigation.ShowMessage("Пользователь удалён");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditEventAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        EditEventViewModel.CurrentEventId = eventItem.EventId;
        _navigation.NavigateTo<EditEventViewModel>();
    }

    private async Task DeleteEventAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        var result = MessageBox.Show($"Удалить мероприятие \"{eventItem.Title}\"?\nВсе записи волонтёров будут отменены.",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;

        try
        {
            await _apiClient.DeleteEventAsync(eventItem.EventId);
            _navigation.ShowMessage("Мероприятие удалено");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ViewVolunteersAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        VolunteersListViewModel.CurrentEventId = eventItem.EventId;
        _navigation.NavigateTo<VolunteersListViewModel>();
    }

    private void Logout()
    {
        _apiClient.ClearAuthToken();
        Application.Current.Properties["CurrentUser"] = null;
        Application.Current.Properties["AuthToken"] = null;
        _navigation.NavigateTo<LoginViewModel>();
    }
}