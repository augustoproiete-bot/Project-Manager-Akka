using System;
using System.Windows.Markup;

namespace LoadingIndicators.WPF
{
    internal class IndicatorVisualStateNames : MarkupExtension
    {
        private static IndicatorVisualStateNames _activeState;
        private static IndicatorVisualStateNames _inactiveState;

        private IndicatorVisualStateNames(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public static IndicatorVisualStateNames ActiveState =>
            _activeState ??= new IndicatorVisualStateNames("Active");

        public static IndicatorVisualStateNames InactiveState =>
            _inactiveState ??= new IndicatorVisualStateNames("Inactive");

        public string Name { get; }

        public override object ProvideValue(IServiceProvider serviceProvider) => Name;
    }
}