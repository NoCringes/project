using System.Windows;
using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private string _email = string.Empty;
    private string _password = string.Empty;

    public LoginViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Вход в систему";
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public RelayCommand LoginCommand => new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
    public RelayCommand GoToRegisterCommand => new RelayCommand(_ => _navigation.NavigateTo<RegisterViewModel>());

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите email и пароль";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new LoginRequest { Email = Email, Password = Password };
            var response = await _apiClient.LoginAsync(request);

            _apiClient.SetAuthToken(response.Token);

            Application.Current.Properties["CurrentUser"] = response;
            Application.Current.Properties["AuthToken"] = response.Token;

            if (response.Role == "admin")
            {
                _navigation.NavigateTo<AdminDashboardViewModel>();
            }
            else if (response.Role == "coordinator")
            {
                _navigation.NavigateTo<CoordinatorDashboardViewModel>();
            }
            else
            {
                _navigation.NavigateTo<VolunteerDashboardViewModel>();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("401"))
            {
                ErrorMessage = "Неверный email или пароль";
            }
            else
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}