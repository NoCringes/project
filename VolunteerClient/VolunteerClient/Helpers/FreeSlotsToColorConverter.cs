using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VolunteerClient.Helpers;

public class FreeSlotsToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int freeSlots)
        {
            if (freeSlots <= 0) return new SolidColorBrush(Colors.Red);
            if (freeSlots <= 3) return new SolidColorBrush(Colors.Orange);
            return new SolidColorBrush(Colors.Green);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}