using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MGIHelper.UI
{
    public sealed class StatusConverter : IMultiValueConverter
    {
        private static readonly StringBuilder Builder = new StringBuilder();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Builder.Clear();

            var internalStart = (bool) values[0];
            var status = values[1] as string;
            var kernel = values[2] is Process;
            var client = values[3] is Process;
            Builder.AppendLine(string.IsNullOrWhiteSpace(status) ? "Unbekannt" : status);

            if (!internalStart)
            {
                Builder.Append("Kernel: ");
                Builder.AppendLine(kernel ? "Gefunden" : "nicht Gefunden");
                Builder.Append("Client: ");
                Builder.AppendLine(client ? "Gefunden" : "nicht Gefunden");
            }
            else
            {
                Builder.Append("Kernel: ");
                Builder.AppendLine(kernel ? "Gestartet" : "nicht Gestartet");
                Builder.Append("Client: ");
                Builder.AppendLine(client ? "Gestartet" : "nicht Gestartet");
            }

            return Builder.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
            => throw new InvalidOperationException();
    }
}