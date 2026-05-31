using System.Windows;
using System.Windows.Controls;

namespace VolunteerClient.Views
{
    // Тестовый класс для мероприятий координатора
    public class CoordinatorEvent
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string SlotsInfo { get; set; } = string.Empty;
    }

    public partial class CoordinatorDashboardView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public CoordinatorDashboardView()
        {
            InitializeComponent();

            _mainWindow = Application.Current.MainWindow as MainWindow;

            CreateEventButton.Click += CreateEventButton_Click;
            RefreshButton.Click += RefreshButton_Click;
            LogoutButton.Click += LogoutButton_Click;

            LoadTestData();
        }

        private void LoadTestData()
        {
            MyEventsList.Items.Add(new CoordinatorEvent
            {
                Title = "Уборка городского парка",
                Location = "г. Москва, Центральный парк",
                Date = "15.06.2026 10:00 - 14:00",
                SlotsInfo = "📋 3 слота, 20 мест"
            });

            MyEventsList.Items.Add(new CoordinatorEvent
            {
                Title = "Помощь приюту для животных",
                Location = "г. Москва, Приют Доброе сердце",
                Date = "20.06.2026 09:00 - 13:00",
                SlotsInfo = "📋 3 слота, 10 мест"
            });

            MyEventsList.Items.Add(new CoordinatorEvent
            {
                Title = "Благотворительный концерт",
                Location = "г. Москва, ДК Мир",
                Date = "10.07.2026 14:00 - 20:00",
                SlotsInfo = "📋 3 слота, 25 мест"
            });
        }

        private void CreateEventButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открытие формы создания мероприятия...\n(будет реализовано позже)",
                            "Создание мероприятия",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обновление списка мероприятий...\n(будет реализовано позже)",
                            "Обновление",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.HideUserPanel();
            var loginView = new LoginView();
            _mainWindow?.MainContent.Content = loginView;
        }
    }
}