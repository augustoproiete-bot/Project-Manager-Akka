using System.Windows.Data;
using System.Windows.Media;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.Localizer.Core.Converter
{
    public sealed class OperationStateConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create()
        {
            return CreateCommonConverter<OperationStatus, SolidColorBrush>(s => s switch
            {
                OperationStatus.Running => Brushes.DarkSlateBlue,
                OperationStatus.Success => Brushes.DarkGreen,
                _ => Brushes.DarkRed
            });
        }
    }
}