using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VolunteerClient.Views
{
    // Модель пользователя
    public class UserViewModel : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        private string _role = string.Empty;
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RoleColor));
            }
        }

        public string RoleColor => Role switch
        {
            "admin" => "#E74C3C",
            "coordinator" => "#F39C12",
            "volunteer" => "#27AE60",
            _ => "#95A5A6"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Модель мероприятия
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Coordinator { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public partial class AdminDashboardView : UserControl
    {
        private readonly MainWindow? _mainWindow;

        public AdminDashboardView()
        {
            InitializeComponent();

            _mainWindow = Application.Current.MainWindow as MainWindow;

            RefreshButton.Click += RefreshButton_Click;
            LogoutButton.Click += LogoutButton_Click;
            CreateEventButton.Click += CreateEventButton_Click;

            LoadTestData();

            // Подписываемся на события после загрузки элементов
            UsersList.Loaded += (s, e) => SubscribeToUserButtons();
            EventsList.Loaded += (s, e) => SubscribeToEventButtons();
        }

        private void LoadTestData()
        {
            // Тестовые пользователи
            var users = new ObservableCollection<UserViewModel>
            {
                new UserViewModel { UserId = 1, FirstName = "Администратор", LastName = "Системы", Email = "admin@example.com", Phone = "+7 (999) 000-00-00", Role = "admin" },
                new UserViewModel { UserId = 2, FirstName = "Анна", LastName = "Козлова", Email = "anna@example.com", Phone = "+7 (999) 456-78-90", Role = "coordinator" },
                new UserViewModel { UserId = 3, FirstName = "Сергей", LastName = "Морозов", Email = "sergey@example.com", Phone = "+7 (999) 567-89-01", Role = "coordinator" },
                new UserViewModel { UserId = 4, FirstName = "Иван", LastName = "Петров", Email = "ivan@example.com", Phone = "+7 (999) 123-45-67", Role = "volunteer" },
                new UserViewModel { UserId = 5, FirstName = "Мария", LastName = "Сидорова", Email = "maria@example.com", Phone = "+7 (999) 234-56-78", Role = "volunteer" },
                new UserViewModel { UserId = 6, FirstName = "Алексей", LastName = "Иванов", Email = "alexey@example.com", Phone = "+7 (999) 345-67-89", Role = "volunteer" }
            };

            UsersList.ItemsSource = users;

            // Тестовые мероприятия
            var events = new ObservableCollection<EventViewModel>
            {
                new EventViewModel { EventId = 1, Title = "Уборка городского парка", Location = "г. Москва, Центральный парк", Date = "15.06.2026 10:00", Coordinator = "Анна Козлова", Status = "active" },
                new EventViewModel { EventId = 2, Title = "Помощь приюту для животных", Location = "г. Москва, Приют Доброе сердце", Date = "20.06.2026 09:00", Coordinator = "Анна Козлова", Status = "active" },
                new EventViewModel { EventId = 3, Title = "Благотворительный концерт", Location = "г. Москва, ДК Мир", Date = "10.07.2026 14:00", Coordinator = "Сергей Морозов", Status = "active" },
                new EventViewModel { EventId = 4, Title = "Посадка деревьев", Location = "г. Москва, ул. Школьная", Date = "01.07.2026 10:00", Coordinator = "Сергей Морозов", Status = "active" }
            };

            EventsList.ItemsSource = events;

            // Обновляем статистику
            UpdateStatistics(users, events);
        }

        private void SubscribeToUserButtons()
        {
            foreach (var item in UsersList.Items)
            {
                var container = UsersList.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    FindButtonsInContainer(container, item as UserViewModel);
                }
            }
        }

        private void SubscribeToEventButtons()
        {
            foreach (var item in EventsList.Items)
            {
                var container = EventsList.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    FindButtonsInEventContainer(container, item as EventViewModel);
                }
            }
        }

        private void FindButtonsInContainer(DependencyObject parent, UserViewModel? user)
        {
            if (user == null) return;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Button button)
                {
                    switch (button.Name)
                    {
                        case "EditUserButton":
                            button.Click += (s, e) => EditUser(user);
                            break;
                        case "ChangeRoleButton":
                            button.Click += (s, e) => ChangeRole(user);
                            break;
                        case "DeleteUserButton":
                            button.Click += (s, e) => DeleteUser(user);
                            break;
                    }
                }
                else
                {
                    FindButtonsInContainer(child, user);
                }
            }
        }

        private void FindButtonsInEventContainer(DependencyObject parent, EventViewModel? evt)
        {
            if (evt == null) return;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Button button)
                {
                    switch (button.Name)
                    {
                        case "EditEventButton":
                            button.Click += (s, e) => EditEvent(evt);
                            break;
                        case "DeleteEventButton":
                            button.Click += (s, e) => DeleteEvent(evt);
                            break;
                        case "ViewVolunteersButton":
                            button.Click += (s, e) => ViewVolunteers(evt);
                            break;
                    }
                }
                else
                {
                    FindButtonsInEventContainer(child, evt);
                }
            }
        }

        // ==================== РЕДАКТИРОВАНИЕ ПОЛЬЗОВАТЕЛЯ ====================

        private void EditUser(UserViewModel user)
        {
            if (user.Role == "admin")
            {
                MessageBox.Show("Нельзя редактировать администратора!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new EditUserDialog(user);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Обновляем отображение в списке
                RefreshUsersList();

                // Обновляем статистику
                var users = UsersList.ItemsSource as ObservableCollection<UserViewModel>;
                UpdateStatistics(users, null);

                MessageBox.Show($"Данные пользователя {user.FirstName} {user.LastName} обновлены!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshUsersList()
        {
            // Принудительно обновляем список
            var users = UsersList.ItemsSource as ObservableCollection<UserViewModel>;
            if (users != null)
            {
                var temp = new ObservableCollection<UserViewModel>(users);
                UsersList.ItemsSource = null;
                UsersList.ItemsSource = temp;
                // Переподписываемся на события
                SubscribeToUserButtons();
            }
        }

        // ==================== УПРАВЛЕНИЕ РОЛЯМИ ====================

        private void ChangeRole(UserViewModel user)
        {
            if (user.Role == "admin")
            {
                MessageBox.Show("Нельзя изменить роль администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newRole = user.Role == "volunteer" ? "coordinator" : "volunteer";

            var result = MessageBox.Show($"Сменить роль пользователя {user.FirstName} {user.LastName} с '{user.Role}' на '{newRole}'?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                user.Role = newRole;
                var users = UsersList.ItemsSource as ObservableCollection<UserViewModel>;
                UpdateStatistics(users, null);
                MessageBox.Show("Роль успешно изменена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ==================== УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ ====================

        private void DeleteUser(UserViewModel user)
        {
            if (user.Role == "admin")
            {
                MessageBox.Show("Нельзя удалить администратора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {user.FirstName} {user.LastName}?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var users = UsersList.ItemsSource as ObservableCollection<UserViewModel>;
                users?.Remove(user);
                UpdateStatistics(users, null);
                MessageBox.Show("Пользователь удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ==================== ОБНОВЛЕНИЕ СТАТИСТИКИ ====================

        private void UpdateStatistics(ObservableCollection<UserViewModel>? users = null, ObservableCollection<EventViewModel>? events = null)
        {
            users ??= UsersList.ItemsSource as ObservableCollection<UserViewModel>;
            events ??= EventsList.ItemsSource as ObservableCollection<EventViewModel>;

            if (users != null)
            {
                TotalUsersText.Text = users.Count.ToString();
                TotalVolunteersText.Text = users.Count(u => u.Role == "volunteer").ToString();
                TotalCoordinatorsText.Text = users.Count(u => u.Role == "coordinator").ToString();
            }

            if (events != null)
            {
                TotalEventsText.Text = events.Count.ToString();
            }
        }

        // ==================== УПРАВЛЕНИЕ МЕРОПРИЯТИЯМИ ====================

        private void EditEvent(EventViewModel evt)
        {
            MessageBox.Show($"Редактирование мероприятия: {evt.Title}\n(будет реализовано позже)",
                            "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteEvent(EventViewModel evt)
        {
            var result = MessageBox.Show($"Удалить мероприятие '{evt.Title}'? Все записи волонтёров будут отменены.",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var events = EventsList.ItemsSource as ObservableCollection<EventViewModel>;
                events?.Remove(evt);
                UpdateStatistics(null, events);
                MessageBox.Show("Мероприятие удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewVolunteers(EventViewModel evt)
        {
            MessageBox.Show($"Список волонтёров на мероприятии: {evt.Title}\n(будет реализовано позже)",
                            "Волонтёры", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================== ОБРАБОТЧИКИ КНОПОК ====================

        private void CreateEventButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Создание нового мероприятия...\n(будет реализовано позже)",
                            "Создание", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Обновление данных...\n(здесь будет загрузка с сервера)",
                            "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow?.HideUserPanel();
            _mainWindow?.MainContent.Content = new LoginView();
        }
    }
}