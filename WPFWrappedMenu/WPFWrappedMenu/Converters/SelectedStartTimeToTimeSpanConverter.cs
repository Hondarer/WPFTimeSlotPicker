using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFWrappedMenu.Converters
{
    public class SelectedStartTimeToTimeSpanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime parsedDateTime)
            {
                return DependencyProperty.UnsetValue;
            }

            return $"{parsedDateTime:HH:mm}–{parsedDateTime.AddMinutes(30):HH:mm}";
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
