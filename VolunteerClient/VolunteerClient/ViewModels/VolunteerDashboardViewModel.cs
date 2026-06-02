using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class VolunteerDashboardViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private ObservableCollection<EventDto> _events = new();
    private ObservableCollection<SlotVolunteerDto> _myApplications = new();
    private EventDto? _selectedEvent;
    private string _searchText = string.Empty;
    public static int CurrentEventId { get; set; }
    public static bool FromMyApplications { get; set; }

    public VolunteerDashboardViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Волонтёрская панель";

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        LogoutCommand = new RelayCommand(_ => Logout());
        ApplyToSlotCommand = new RelayCommand<int?>(async slotId => await ApplyToSlotAsync(slotId));
        CancelApplicationCommand = new RelayCommand<int?>(async recordId => await CancelApplicationAsync(recordId));
        MarkAttendanceCommand = new RelayCommand<int?>(async recordId => await MarkAttendanceAsync(recordId));
        OpenEventDetailsCommand = new RelayCommand<EventDto?>(async eventItem => await OpenEventDetailsAsync(eventItem));
        OpenEventFromMyApplicationsCommand = new RelayCommand<SlotVolunteerDto?>(async application => await OpenEventFromMyApplicationsAsync(application));

        Task.Run(async () => await LoadDataAsync());
    }

    public ObservableCollection<EventDto> Events
    {
        get => _events;
        set => SetProperty(ref _events, value);
    }

    public ObservableCollection<SlotVolunteerDto> MyApplications
    {
        get => _myApplications;
        set => SetProperty(ref _myApplications, value);
    }

    public EventDto? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            SetProperty(ref _selectedEvent, value);
            if (value != null)
            {
                OpenEventDetailsCommand.Execute(value);
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterEvents();
        }
    }

    public RelayCommand LoadDataCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand<int?> ApplyToSlotCommand { get; }
    public RelayCommand<int?> CancelApplicationCommand { get; }
    public RelayCommand<int?> MarkAttendanceCommand { get; }
    public RelayCommand<EventDto?> OpenEventDetailsCommand { get; }
    public RelayCommand<SlotVolunteerDto?> OpenEventFromMyApplicationsCommand { get; }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var events = await _apiClient.GetEventsAsync();

            var allApplications = await _apiClient.GetMyApplicationsAsync();

            var registeredSlotIds = allApplications
                .Where(a => a.Status == "registered")
                .Select(a => a.SlotId)
                .ToHashSet();

            foreach (var eventItem in events)
            {
                foreach (var slot in eventItem.Slots)
                {
                    slot.IsUserRegistered = registeredSlotIds.Contains(slot.SlotId);
                }
            }

            Events = new ObservableCollection<EventDto>(events);

            var activeApplications = allApplications.Where(a => a.Status == "registered").ToList();
            MyApplications = new ObservableCollection<SlotVolunteerDto>(activeApplications);
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

    private void FilterEvents()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Task.Run(async () => await LoadDataAsync());
            return;
        }

        var filtered = _events.Where(e =>
            e.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            e.Location.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Events = new ObservableCollection<EventDto>(filtered);
    }

    private async Task OpenEventDetailsAsync(EventDto? eventItem)
    {
        if (eventItem is null) return;

        EventDetailsViewModel.CurrentEventId = eventItem.EventId;
        EventDetailsViewModel.FromMyApplications = false;

        _navigation.NavigateTo<EventDetailsViewModel>();
    }

    private async Task OpenEventFromMyApplicationsAsync(SlotVolunteerDto? application)
    {
        if (application is null) return;

        EventDetailsViewModel.CurrentEventId = application.EventId;
        EventDetailsViewModel.FromMyApplications = true;

        _navigation.NavigateTo<EventDetailsViewModel>();
    }

    private async Task ApplyToSlotAsync(int? slotId)
    {
        if (!slotId.HasValue) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _apiClient.ApplyToSlotAsync(slotId.Value);
            _navigation.ShowMessage("Вы успешно записались на мероприятие!");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("400"))
            {
                ErrorMessage = "Нет свободных мест или вы уже записаны";
            }
            else
            {
                ErrorMessage = $"Ошибка записи: {ex.Message}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelApplicationAsync(int? recordId)
    {
        if (!recordId.HasValue) return;

        var result = MessageBox.Show("Отменить запись?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _apiClient.CancelApplicationAsync(recordId.Value);
            _navigation.ShowMessage("Запись отменена");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка отмены: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkAttendanceAsync(int? recordId)
    {
        if (!recordId.HasValue) return;

        var result = MessageBox.Show("Отметить своё участие на мероприятии?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _apiClient.MarkAttendanceAsync(recordId.Value);
            _navigation.ShowMessage("Участие отмечено! Спасибо, что пришли!");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Logout()
    {
        _apiClient.ClearAuthToken();
        Application.Current.Properties["CurrentUser"] = null;
        Application.Current.Properties["AuthToken"] = null;
        _navigation.NavigateTo<LoginViewModel>();
    }
}