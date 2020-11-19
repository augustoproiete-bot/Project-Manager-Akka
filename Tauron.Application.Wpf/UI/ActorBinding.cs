using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public class ActorBinding : BindingDecoratorBase
    {
        private readonly string _name;

        public ActorBinding(string name) => _name = name;

        public override object? ProvideValue(IServiceProvider provider)
        {
            try
            {
                if (!TryGetTargetItems(provider, out var dependencyObject, out _))
                    return DependencyProperty.UnsetValue;

                if (DesignerProperties.GetIsInDesignMode(dependencyObject))
                    return DependencyProperty.UnsetValue;

                var context = ControlBindLogic.FindDataContext(dependencyObject.ToMaybe());
                if (context.IsNothing()) return null;

                Path                                = Path != null ? new PropertyPath("Value." + Path.Path, Path.PathParameters) : new PropertyPath("Value");
                Source                              = new DeferredSource(_name, context);
                Binding.Delay                       = 500;
                UpdateSourceTrigger                 = UpdateSourceTrigger.PropertyChanged;
                Binding.ValidatesOnNotifyDataErrors = true;

                return base.ProvideValue(provider);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}