using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Akka.MGIHelper.UI.FanControl
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;

            if ((bool) value) return Brushes.Green;
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}