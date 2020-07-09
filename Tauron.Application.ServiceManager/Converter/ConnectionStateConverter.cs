using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.ServiceManager.Converter
{
    public sealed class ConnectionStateConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() 
            => CreateCommonConverter<ConnectionState, Brush>(
                cs => cs switch
                {
                    ConnectionState.Offline => Brushes.Transparent,
                    ConnectionState.Connecting => Brushes.Blue,
                    ConnectionState.Online => Brushes.Green,
                    _ => Brushes.Transparent
                });
    }
}