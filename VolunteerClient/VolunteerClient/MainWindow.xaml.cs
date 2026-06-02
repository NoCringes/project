using System.Windows;
using System.Windows.Controls;

namespace VolunteerClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetContent(UserControl content)
    {
        MainContent.Content = content;
    }
}