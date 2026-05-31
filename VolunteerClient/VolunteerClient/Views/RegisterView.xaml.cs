using System.Windows;
using System.Windows.Controls;

namespace VolunteerClient.Views
{
    public partial class RegisterView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public RegisterView()
        {
            InitializeComponent();

            _mainWindow = Application.Current.MainWindow as MainWindow;

            RegisterButton.Click += RegisterButton_Click;
            GoToLoginButton.Click += GoToLoginButton_Click;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string role = VolunteerRole.IsChecked == true ? "volunteer" : "coordinator";

            MessageBox.Show($"Регистрация: {FirstNameBox.Text} {LastNameBox.Text}\n" +
                           $"Email: {EmailBox.Text}\n" +
                           $"Телефон: {PhoneBox.Text}\n" +
                           $"Роль: {role}\n\n(здесь будет реальная регистрация)",
                           "Информация",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);

            // Возвращаемся на вход
            _mainWindow?.MainContent?.Content?.GetType();
            _mainWindow?.MainContent.Content = new LoginView();
        }

        private void GoToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.MainContent?.Content?.GetType();
            _mainWindow?.MainContent.Content = new LoginView();
        }
    }
}