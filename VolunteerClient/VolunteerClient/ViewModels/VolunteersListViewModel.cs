using System.Collections.ObjectModel;
using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class VolunteersListViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    public static int CurrentEventId { get; set; }

    private ObservableCollection<SlotWithVolunteersDto> _slots = new();
    private string _eventTitle = string.Empty;
    private string _eventLocation = string.Empty;
    private DateTime _eventStartDateTime;

    public VolunteersListViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Волонтёры мероприятия";

        BackCommand = new RelayCommand(_ => Back());
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => !IsBusy);
        ConfirmAttendanceCommand = new RelayCommand<SlotVolunteerDto?>(async volunteer => await ConfirmAttendanceAsync(volunteer), _ => !IsBusy);

        Task.Run(async () => await LoadDataAsync());
    }

    public ObservableCollection<SlotWithVolunteersDto> Slots
    {
        get => _slots;
        set => SetProperty(ref _slots, value);
    }

    public string EventTitle
    {
        get => _eventTitle;
        set => SetProperty(ref _eventTitle, value);
    }

    public string EventLocation
    {
        get => _eventLocation;
        set => SetProperty(ref _eventLocation, value);
    }

    public DateTime EventStartDateTime
    {
        get => _eventStartDateTime;
        set => SetProperty(ref _eventStartDateTime, value);
    }

    public RelayCommand BackCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand<SlotVolunteerDto?> ConfirmAttendanceCommand { get; }

    private async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var slots = await _apiClient.GetEventRegistrationsAsync(CurrentEventId);
            Slots = new ObservableCollection<SlotWithVolunteersDto>(slots);

            var eventItem = await _apiClient.GetEventAsync(CurrentEventId);
            EventTitle = eventItem.Title;
            EventLocation = eventItem.Location;
            EventStartDateTime = eventItem.StartDateTime;

            Title = $"Волонтёры: {EventTitle}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ConfirmAttendanceAsync(SlotVolunteerDto? volunteer)
    {
        if (volunteer is null) return;

        var result = MessageBox.Show($"Подтвердить участие волонтёра {volunteer.VolunteerName}?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;

        try
        {
            await _apiClient.ConfirmAttendanceAsync(volunteer.RecordId);
            _navigation.ShowMessage("Участие подтверждено!");
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

    private void Back()
    {
        CurrentEventId = 0;
        _navigation.GoBack();
    }
}