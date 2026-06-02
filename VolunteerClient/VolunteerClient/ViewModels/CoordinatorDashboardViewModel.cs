using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class CoordinatorDashboardViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private ObservableCollection<EventDto> _myEvents = new();
    private EventDto? _selectedEvent;

    public CoordinatorDashboardViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Панель координатора";

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => !IsBusy);
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => !IsBusy);
        LogoutCommand = new RelayCommand(_ => Logout());
        CreateEventCommand = new RelayCommand(_ => _navigation.NavigateTo<CreateEventViewModel>());
        EditEventCommand = new RelayCommand<EventDto?>(async eventItem => await EditEventAsync(eventItem), _ => !IsBusy);
        DeleteEventCommand = new RelayCommand<EventDto?>(async eventItem => await DeleteEventAsync(eventItem), _ => !IsBusy);
        ViewRegistrationsCommand = new RelayCommand<EventDto?>(async eventItem => await ViewRegistrationsAsync(eventItem), _ => !IsBusy);

        Task.Run(async () => await LoadDataAsync());
    }

    public ObservableCollection<EventDto> MyEvents
    {
        get => _myEvents;
        set => SetProperty(ref _myEvents, value);
    }

    public EventDto? SelectedEvent
    {
        get => _selectedEvent;
        set => SetProperty(ref _selectedEvent, value);
    }

    public RelayCommand LoadDataCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand CreateEventCommand { get; }
    public RelayCommand<EventDto?> EditEventCommand { get; }
    public RelayCommand<EventDto?> DeleteEventCommand { get; }
    public RelayCommand<EventDto?> ViewRegistrationsCommand { get; }

    private int GetCurrentUserId()
    {
        if (Application.Current.Properties["CurrentUser"] is AuthResponse user)
        {
            return user.UserId;
        }
        return 0;
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var allEvents = await _apiClient.GetEventsAsync();
            var currentUserId = GetCurrentUserId();

            var myEvents = allEvents.Where(e => e.CreatedBy == currentUserId).ToList();
            MyEvents = new ObservableCollection<EventDto>(myEvents);
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

    private async Task EditEventAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        // Передаём ID мероприятия в статическое поле
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

    private async Task ViewRegistrationsAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        VolunteersListViewModel.CurrentEventId = eventItem.EventId;
        VolunteersListViewModel.CurrentEventTitle = eventItem.Title;
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