using System;
using System.Globalization;
using System.Windows.Data;

namespace Tauron.Application.Wpf.UI
{
    //[PublicAPI]
    //public sealed class ViewModelConverterExtension : ValueConverterFactoryBase
    //{
    //    protected override IValueConverter Create() => new ViewModelConverter();
    //}

    public class ViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(parameter is IView root)) return value;
            if (!(value is IViewModel model)) return value;

            //var view = root.ViewManager.Get(model, root);
            //if (view != null)
            //    return view;

            var manager = AutoViewLocation.Manager;
            var view = manager.ResolveView(model);
            if (view == null)
                return value;

            //root.ViewManager.Register(model, view, root);
            return view;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new FrameworkObject(value).DataContext;
    }
}