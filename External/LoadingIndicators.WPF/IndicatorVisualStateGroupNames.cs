using System;
using System.Windows.Markup;

namespace LoadingIndicators.WPF
{
    internal class IndicatorVisualStateGroupNames : MarkupExtension
    {
        private static IndicatorVisualStateGroupNames _internalActiveStates;
        private static IndicatorVisualStateGroupNames _sizeStates;

        private IndicatorVisualStateGroupNames(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public static IndicatorVisualStateGroupNames ActiveStates =>
            _internalActiveStates ??= new IndicatorVisualStateGroupNames("ActiveStates");

        public static IndicatorVisualStateGroupNames SizeStates =>
            _sizeStates ??= new IndicatorVisualStateGroupNames("SizeStates");

        public string Name { get; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Name;
        }
    }
}