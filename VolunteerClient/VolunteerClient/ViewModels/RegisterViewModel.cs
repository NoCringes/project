using VolunteerClient.Helpers;
using VolunteerClient.Models;
using VolunteerClient.Services;

namespace VolunteerClient.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;
    private readonly NavigationService _navigation;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _phone = string.Empty;
    private string _role = "volunteer";
    private bool _isVolunteer = true;
    private bool _isCoordinator = false;

    public RegisterViewModel(ApiClient apiClient, NavigationService navigation)
    {
        _apiClient = apiClient;
        _navigation = navigation;
        Title = "Регистрация";
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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public bool IsVolunteer
    {
        get => _isVolunteer;
        set
        {
            if (SetProperty(ref _isVolunteer, value) && value)
            {
                Role = "volunteer";
                IsCoordinator = false;
            }
        }
    }

    public bool IsCoordinator
    {
        get => _isCoordinator;
        set
        {
            if (SetProperty(ref _isCoordinator, value) && value)
            {
                Role = "coordinator";
                IsVolunteer = false;
            }
        }
    }

    public RelayCommand RegisterCommand => new RelayCommand(async _ => await RegisterAsync(), _ => !IsBusy);
    public RelayCommand GoToLoginCommand => new RelayCommand(_ => _navigation.NavigateTo<LoginViewModel>());

    private async Task RegisterAsync()
    {
        // Валидация
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Заполните все обязательные поля";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Пароль должен быть не менее 6 символов";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new RegisterRequest
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone,
                Role = Role
            };

            var response = await _apiClient.RegisterAsync(request);

            _navigation.ShowMessage($"Пользователь {response.FirstName} {response.LastName} успешно зарегистрирован!", "Успех");

            _navigation.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("409") || ex.Message.Contains("already exists"))
            {
                ErrorMessage = "Пользователь с таким email уже существует";
            }
            else
            {
                ErrorMessage = $"Ошибка регистрации: {ex.Message}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}