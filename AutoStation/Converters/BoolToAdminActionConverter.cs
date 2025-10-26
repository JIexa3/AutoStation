using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoStation.Converters
{
    public class BoolToAdminActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAdmin)
            {
                return isAdmin ? "Отменить права" : "Сделать админом";
            }
            return "Сделать админом";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
