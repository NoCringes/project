using System.Windows;
using System.Windows.Controls;

namespace VolunteerClient.Views
{
    public partial class VolunteerDashboardView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public VolunteerDashboardView()
        {
            InitializeComponent();

            _mainWindow = Application.Current.MainWindow as MainWindow;

            LogoutButton.Click += LogoutButton_Click;
            RefreshButton.Click += RefreshButton_Click;

            // Добавляем тестовые данные для отображения
            LoadTestData();
        }

        private void LoadTestData()
        {
            // Добавляем тестовые элементы в список мероприятий
            for (int i = 1; i <= 5; i++)
            {
                EventsList.Items.Add(new { });
            }

            // Добавляем тестовые элементы в список моих записей
            for (int i = 1; i <= 3; i++)
            {
                MyRegistrationsList.Items.Add(new { });
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.HideUserPanel();

            // Очищаем содержимое и показываем форму входа
            var loginView = new LoginView();
            _mainWindow?.MainContent.Content = loginView;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обновление данных... (здесь будет загрузка с сервера)",
                            "Обновление",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}