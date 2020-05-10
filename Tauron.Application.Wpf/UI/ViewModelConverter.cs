using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class ViewModelConverterExtension : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => new ViewModelConverter();
    }

    public class ViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IViewModel model)) return value;

            var manager = AutoViewLocation.Manager;

            return manager.ResolveView(model) ?? value;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new FrameworkObject(value).DataContext;
    }
}