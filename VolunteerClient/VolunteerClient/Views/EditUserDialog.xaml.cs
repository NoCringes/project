using System.Windows;
using VolunteerClient.Views;

namespace VolunteerClient.Views
{
    public partial class EditUserDialog : Window
    {
        public UserViewModel User { get; private set; }
        public bool IsSaved { get; private set; } = false;

        public EditUserDialog(UserViewModel user)
        {
            InitializeComponent();
            User = user;

            // Заполняем поля
            FirstNameBox.Text = user.FirstName;
            LastNameBox.Text = user.LastName;
            EmailBox.Text = user.Email;
            PhoneBox.Text = user.Phone;

            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем данные пользователя
            User.FirstName = FirstNameBox.Text;
            User.LastName = LastNameBox.Text;
            User.Email = EmailBox.Text;
            User.Phone = PhoneBox.Text;

            IsSaved = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}