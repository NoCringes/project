using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class EditEventViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    public static int CurrentEventId { get; set; }

    private int _eventId;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _location = string.Empty;
    private DateTime _startDateTime;
    private DateTime _endDateTime;
    private int _maxVolunteers;
    private ObservableCollection<EditSlotViewModel> _slots = new();

    public EditEventViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Редактирование мероприятия";

        // Кнопка сохранить всегда активна
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => true);
        CancelCommand = new RelayCommand(_ => Cancel());
        AddSlotCommand = new RelayCommand(_ => AddSlot());
        RemoveSlotCommand = new RelayCommand<EditSlotViewModel?>(slot => RemoveSlot(slot!), _ => true);

        Task.Run(async () => await LoadDataAsync());
    }

    public string Title
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

    public ObservableCollection<EditSlotViewModel> Slots
    {
        get => _slots;
        set => SetProperty(ref _slots, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand AddSlotCommand { get; }
    public RelayCommand<EditSlotViewModel?> RemoveSlotCommand { get; }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var eventItem = await _apiClient.GetEventAsync(CurrentEventId);

            _eventId = eventItem.EventId;
            Title = eventItem.Title;
            Description = eventItem.Description ?? string.Empty;
            Location = eventItem.Location;
            StartDateTime = eventItem.StartDateTime.ToLocalTime();
            EndDateTime = eventItem.EndDateTime.ToLocalTime();
            MaxVolunteers = eventItem.MaxVolunteers;

            var newSlots = new ObservableCollection<EditSlotViewModel>();
            foreach (var slot in eventItem.Slots)
            {
                newSlots.Add(new EditSlotViewModel
                {
                    Title = slot.Title,
                    Description = slot.Description ?? string.Empty,
                    SlotsAvailable = slot.SlotsAvailable
                });
            }

            Slots = newSlots;
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

    private void AddSlot()
    {
        Slots.Add(new EditSlotViewModel
        {
            Title = "Новый слот",
            Description = "",
            SlotsAvailable = 1
        });
    }

    private void RemoveSlot(EditSlotViewModel slot)
    {
        if (Slots.Count > 1)
        {
            Slots.Remove(slot);
        }
        else
        {
            _navigation.ShowMessage("Должен быть хотя бы один слот");
        }
    }

    private async Task SaveAsync()
    {
        // Простая проверка перед сохранением
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Введите название мероприятия";
            return;
        }

        if (string.IsNullOrWhiteSpace(Location))
        {
            ErrorMessage = "Введите место проведения";
            return;
        }

        if (StartDateTime >= EndDateTime)
        {
            ErrorMessage = "Дата окончания должна быть позже даты начала";
            return;
        }

        if (MaxVolunteers <= 0)
        {
            ErrorMessage = "Максимальное количество волонтёров должно быть больше 0";
            return;
        }

        if (Slots.Count == 0)
        {
            ErrorMessage = "Добавьте хотя бы один слот";
            return;
        }

        foreach (var slot in Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Title))
            {
                ErrorMessage = "Заполните название всех слотов";
                return;
            }
            if (slot.SlotsAvailable <= 0)
            {
                ErrorMessage = "Количество мест в слоте должно быть больше 0";
                return;
            }
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new CreateEventRequest
            {
                Title = Title,
                Description = Description,
                Location = Location,
                StartDateTime = StartDateTime.ToUniversalTime(),
                EndDateTime = EndDateTime.ToUniversalTime(),
                MaxVolunteers = MaxVolunteers,
                Slots = Slots.Select(s => new CreateSlotRequest
                {
                    Title = s.Title,
                    Description = s.Description,
                    SlotsAvailable = s.SlotsAvailable
                }).ToList()
            };

            await _apiClient.UpdateEventAsync(_eventId, request);
            _navigation.ShowMessage("Мероприятие успешно обновлено!");
            _navigation.GoBack();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        var result = MessageBox.Show("Отменить изменения? Несохранённые данные будут потеряны.",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _navigation.GoBack();
        }
    }
}

public class EditSlotViewModel : BaseViewModel
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private int _slotsAvailable = 1;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public int SlotsAvailable
    {
        get => _slotsAvailable;
        set => SetProperty(ref _slotsAvailable, value);
    }
}