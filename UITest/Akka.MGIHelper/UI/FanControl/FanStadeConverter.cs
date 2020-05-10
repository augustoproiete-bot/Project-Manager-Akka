using System;
using System.Globalization;
using System.Windows.Data;

namespace Akka.MGIHelper.UI.FanControl
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class FanStadeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Keine Daten";

            if ((bool) value) return "Externer Lüfter Läuft";
            return "Externer Lüfter Steht";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}