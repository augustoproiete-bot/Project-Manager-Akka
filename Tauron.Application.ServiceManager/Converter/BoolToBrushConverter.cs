using System.Drawing;
using System.Windows.Data;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.ServiceManager.Converter
{
    public sealed class BoolToBrushConverter: ValueConverterFactoryBase
    {
        protected override IValueConverter Create() 
            => CreateCommonConverter<bool, Brush>(b => b ? Brushes.Green : Brushes.Red);
    }
}