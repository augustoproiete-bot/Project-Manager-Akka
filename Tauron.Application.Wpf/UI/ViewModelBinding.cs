using System;
using System.Windows;
using System.Windows.Markup;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class ViewModelBinding : UpdatableMarkupExtension
    {
        private readonly string _name;

        public ViewModelBinding(string name) 
            => _name = name;

        //public override object? ProvideValue(IServiceProvider provider)
        //{
        //    if (!TryGetTargetItems(provider, out var target, out _))
        //        return null;

        //    if (target is Window window)
        //        ConverterParameter = window;
        //    else
        //        ConverterParameter = ControlBindLogic.FindParentView(target);

        //    return base.ProvideValue(provider);
        //}

        protected override object DesignTime() => "Design Time: No View";

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)) 
                return "Invalid IProvideValueTarget: " + _name;
            if (!(service.TargetObject is DependencyObject target))
                return "Invalid Target Object: " + _name;
            if (!(ControlBindLogic.FindDataContext(target, out var promise)))
                return "No Data Context Found: " + _name;
            if (!(ControlBindLogic.FindRoot(target) is IView view))
                return "No View as Root: " + _name;

            var connector = new ViewConnector(_name, promise, view.ViewManager, view, UpdateValue, target.Dispatcher);

            return connector;
        }
    }
}