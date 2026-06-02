using System.Windows.Controls;

namespace VolunteerClient.Views;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                PasswordBox.PasswordChanged += (_, _) => vm.Password = PasswordBox.Password;
                ConfirmPasswordBox.PasswordChanged += (_, _) => vm.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        };
    }
}