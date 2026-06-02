using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class CreateEventViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _location = string.Empty;
    private DateTime _startDateTime = DateTime.Now.AddDays(7);
    private DateTime _endDateTime = DateTime.Now.AddDays(7).AddHours(4);
    private int _maxVolunteers = 10;
    private ObservableCollection<SlotViewModel> _slots = new();

    public CreateEventViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Создание мероприятия";

        // Кнопка активна всегда, кроме момента сохранения
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
        CancelCommand = new RelayCommand(_ => Cancel());
        AddSlotCommand = new RelayCommand(_ => AddSlot());
        RemoveSlotCommand = new RelayCommand<SlotViewModel?>(slot => RemoveSlot(slot!), _ => !IsBusy);

        // Добавляем первый слот с заполненными данными
        AddSlot();
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

    public ObservableCollection<SlotViewModel> Slots
    {
        get => _slots;
        set => SetProperty(ref _slots, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand AddSlotCommand { get; }
    public RelayCommand<SlotViewModel?> RemoveSlotCommand { get; }

    private void AddSlot()
    {
        Slots.Add(new SlotViewModel
        {
            Title = "Новый слот",
            Description = "",
            SlotsAvailable = 1
        });
    }

    private void RemoveSlot(SlotViewModel slot)
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

            var result = await _apiClient.CreateEventAsync(request);

            _navigation.ShowMessage($"Мероприятие \"{result.Title}\" успешно создано!");
            _navigation.NavigateTo<CoordinatorDashboardViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка создания: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        var result = MessageBox.Show("Отменить создание мероприятия? Введённые данные будут потеряны.",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _navigation.NavigateTo<CoordinatorDashboardViewModel>();
        }
    }
}

public class SlotViewModel : BaseViewModel
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private int _slotsAvailable = 1;

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

    public int SlotsAvailable
    {
        get => _slotsAvailable;
        set => SetProperty(ref _slotsAvailable, value);
    }
}