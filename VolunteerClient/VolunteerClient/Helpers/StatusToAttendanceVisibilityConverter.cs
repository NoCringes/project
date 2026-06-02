using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VolunteerClient.Helpers;

public class StatusToAttendanceVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value as string;
        // Показываем кнопку "Отметиться" только для статуса "registered"
        return status == "registered" ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}