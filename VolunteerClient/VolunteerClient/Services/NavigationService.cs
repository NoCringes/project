using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using VolunteerClient.ViewModels;

namespace VolunteerClient.Services;

public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Window? _mainWindow;
    private Stack<Type> _navigationHistory = new Stack<Type>();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModels.BaseViewModel
    {
        if (_mainWindow is null) return;

        // Сохраняем текущую страницу в историю
        if (_mainWindow.Content is UserControl currentView && currentView.DataContext != null)
        {
            var currentVmType = currentView.DataContext.GetType();
            if (_navigationHistory.Count == 0 || _navigationHistory.Peek() != currentVmType)
            {
                _navigationHistory.Push(currentVmType);
            }
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        var viewName = typeof(TViewModel).Name.Replace("ViewModel", "View");
        var viewType = Type.GetType($"VolunteerClient.Views.{viewName}");

        if (viewType is null)
        {
            throw new Exception($"View for {typeof(TViewModel).Name} not found");
        }

        var view = Activator.CreateInstance(viewType) as UserControl;
        if (view is null) return;

        view.DataContext = viewModel;
        _mainWindow.Content = view;
        _mainWindow.Title = viewModel.Title;
    }

    public void GoBack()
    {
        if (_navigationHistory.Count > 0)
        {
            var previousViewModelType = _navigationHistory.Pop();

            var viewModel = _serviceProvider.GetRequiredService(previousViewModelType);
            var viewName = previousViewModelType.Name.Replace("ViewModel", "View");
            var viewType = Type.GetType($"VolunteerClient.Views.{viewName}");

            if (viewType is null) return;

            var view = Activator.CreateInstance(viewType) as UserControl;
            if (view is null) return;

            view.DataContext = viewModel;
            if (_mainWindow != null)
            {
                _mainWindow.Content = view;
                if (viewModel is BaseViewModel vm)
                {
                    _mainWindow.Title = vm.Title;
                }
            }
        }
    }

    public void ShowMessage(string message, string title = "Информация")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Ошибка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}