using System.Windows.Data;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.Localizer.Core.Converter
{
    public sealed class OperationBadgetConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create()
        {
            return CreateCommonConverter<int, object?>(ConvertInt);
        }

        private static object? ConvertInt(int arg)
        {
            if (arg == 0)
                return null;
            return arg;
        }
    }
}