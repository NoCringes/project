using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class EventDetailsViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    // Статические поля для передачи данных между ViewModel
    public static int CurrentEventId { get; set; }
    public static bool FromMyApplications { get; set; }

    private EventDto? _event;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _location = string.Empty;
    private DateTime _startDateTime;
    private DateTime _endDateTime;
    private int _maxVolunteers;
    private string _status = string.Empty;
    private ObservableCollection<EventSlotDto> _slots = new();
    private EventSlotDto? _selectedSlot;
    private bool _isAlreadyRegisteredForAnySlot;
    private bool _canCancel;

    public EventDetailsViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Детали мероприятия";

        LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
        BackCommand = new RelayCommand(_ => Back());
        ApplyToSlotCommand = new RelayCommand<int?>(async slotId => await ApplyToSlotAsync(slotId));
        CancelRegistrationCommand = new RelayCommand(async _ => await CancelRegistrationAsync());

        if (CurrentEventId > 0)
        {
            Task.Run(async () => await LoadDataAsync());
        }
    }

    public new string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public DateTime StartDateTime
    {
        get => _startDateTime;
        set => SetProperty(ref _startDateTime, value);
    }

    public DateTime EndDateTime
    {
        get => _endDateTime;
        set => SetProperty(ref _endDateTime, value);
    }

    public int MaxVolunteers
    {
        get => _maxVolunteers;
        set => SetProperty(ref _maxVolunteers, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public ObservableCollection<EventSlotDto> Slots
    {
        get => _slots;
        set => SetProperty(ref _slots, value);
    }

    public EventSlotDto? SelectedSlot
    {
        get => _selectedSlot;
        set => SetProperty(ref _selectedSlot, value);
    }

    public bool IsAlreadyRegisteredForAnySlot
    {
        get => _isAlreadyRegisteredForAnySlot;
        set => SetProperty(ref _isAlreadyRegisteredForAnySlot, value);
    }

    public bool CanCancel
    {
        get => _canCancel;
        set => SetProperty(ref _canCancel, value);
    }

    public RelayCommand LoadDataCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand<int?> ApplyToSlotCommand { get; }
    public RelayCommand CancelRegistrationCommand { get; }

    private async Task<bool> IsUserAlreadyRegisteredForAnySlot()
    {
        var myApplications = await _apiClient.GetMyApplicationsAsync();
        var registeredSlotIds = myApplications
            .Where(a => a.Status == "registered")
            .Select(a => a.SlotId)
            .ToHashSet();

        return _slots.Any(s => registeredSlotIds.Contains(s.SlotId));
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var eventItem = await _apiClient.GetEventAsync(CurrentEventId);
            _event = eventItem;

            Title = eventItem.Title;
            Description = eventItem.Description ?? "Нет описания";
            Location = eventItem.Location;
            StartDateTime = eventItem.StartDateTime;
            EndDateTime = eventItem.EndDateTime;
            MaxVolunteers = eventItem.MaxVolunteers;
            Status = eventItem.Status;

            var myApplications = await _apiClient.GetMyApplicationsAsync();
            var registeredSlotIds = myApplications
                .Where(a => a.Status == "registered")
                .Select(a => a.SlotId)
                .ToHashSet();

            foreach (var slot in eventItem.Slots)
            {
                slot.IsUserRegistered = registeredSlotIds.Contains(slot.SlotId);
            }

            Slots = new ObservableCollection<EventSlotDto>(eventItem.Slots);

            IsAlreadyRegisteredForAnySlot = await IsUserAlreadyRegisteredForAnySlot();

            // Проверяем, записан ли пользователь на это мероприятие (для кнопки отмены)
            var userRegistration = myApplications.FirstOrDefault(a => a.EventId == CurrentEventId && a.Status == "registered");
            CanCancel = userRegistration != null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки мероприятия: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ApplyToSlotAsync(int? slotId)
    {
        if (!slotId.HasValue) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (IsAlreadyRegisteredForAnySlot)
            {
                ErrorMessage = "❌ Вы уже записаны на другой слот этого мероприятия. Нельзя записываться на несколько слотов.";
                return;
            }

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

    private async Task CancelRegistrationAsync()
    {
        var result = MessageBox.Show("Отменить запись на это мероприятие?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;

        try
        {
            // Получаем все записи пользователя
            var myApplications = await _apiClient.GetMyApplicationsAsync();

            // Ищем запись на ТЕКУЩЕЕ мероприятие (CurrentEventId) со статусом "registered"
            var registration = myApplications.FirstOrDefault(a =>
                a.EventId == CurrentEventId && a.Status == "registered");

            if (registration != null)
            {
                await _apiClient.CancelApplicationAsync(registration.RecordId);
                _navigation.ShowMessage("Запись отменена");
                await LoadDataAsync(); // Обновляем данные
                CanCancel = false;
            }
            else
            {
                // Если не нашли активную запись
                ErrorMessage = "Активная запись на это мероприятие не найдена";
            }
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

    private void Back()
    {
        CurrentEventId = 0;
        FromMyApplications = false;
        _navigation.NavigateTo<VolunteerDashboardViewModel>();
    }
}