using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VolunteerClient.Services;
using VolunteerClient.ViewModels;
using VolunteerClient.Views;

namespace VolunteerClient;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // ========== Регистрация сервисов (синглтоны) ==========
        services.AddSingleton<ApiClient>();
        services.AddSingleton<NavigationService>();

        // ========== Регистрация ViewModels (transient) ==========
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<VolunteerDashboardViewModel>();
        services.AddTransient<CoordinatorDashboardViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<CreateEventViewModel>();
        services.AddTransient<EventDetailsViewModel>();  // ← ДОБАВИТЬ ЭТУ СТРОКУ
        services.AddTransient<EditEventViewModel>();
        services.AddTransient<VolunteersListViewModel>();


        // ========== Регистрация Views (transient) ==========
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<VolunteerDashboardView>();
        services.AddTransient<CoordinatorDashboardView>();
        services.AddTransient<AdminDashboardView>();
        services.AddTransient<CreateEventView>();
        services.AddTransient<EventDetailsView>();  // ← ДОБАВИТЬ ЭТУ СТРОКУ
        services.AddTransient<EditEventView>();
        services.AddTransient<VolunteersListView>();



        _serviceProvider = services.BuildServiceProvider();

        var navigation = _serviceProvider.GetRequiredService<NavigationService>();
        var mainWindow = new MainWindow();

        navigation.Initialize(mainWindow);
        navigation.NavigateTo<LoginViewModel>();

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}