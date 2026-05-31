using System.Windows;
using System.Windows.Controls;

namespace VolunteerClient.Views
{
    public partial class LoginView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public LoginView()
        {
            InitializeComponent();

            // Находим главное окно
            _mainWindow = Application.Current.MainWindow as MainWindow;

            // Привязываем обработчики
            LoginButton.Click += LoginButton_Click;
            GoToRegisterButton.Click += GoToRegisterButton_Click;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var role = "admin";
            //var role = "coordinator";
            //var role = "volunteer"; 

            if (role == "admin")
            {
                _mainWindow?.ShowUserPanel("Администратор (admin)");
                _mainWindow?.MainContent.Content = new AdminDashboardView();
            }
            else if (role == "coordinator")
            {
                _mainWindow?.ShowUserPanel("Анна Козлова (coordinator)");
                _mainWindow?.MainContent.Content = new CoordinatorDashboardView();
            }
            else if (role == "volunteer")
            {
                _mainWindow?.ShowUserPanel("Иван Петров (volunteer)");
                _mainWindow?.MainContent.Content = new VolunteerDashboardView();
            }
        }
        private void GoToRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.MainContent.Content = new RegisterView();

        }
    }
}