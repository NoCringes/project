using System.Windows;
using VolunteerClient.Views;

namespace VolunteerClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainContent.Content = new LoginView();
        }

        // Временный метод для отображения имени пользователя (пока заглушка)
        public void ShowUserPanel(string userName)
        {
            UserNameText.Text = userName;
            UserPanel.Visibility = Visibility.Visible;
        }

        public void HideUserPanel()
        {
            UserPanel.Visibility = Visibility.Collapsed;
        }
    }
}