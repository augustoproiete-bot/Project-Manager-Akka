using System;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public class ActorBinding : BindingDecoratorBase
    {
        private readonly string _name;

        public ActorBinding(string name) 
            => _name = name;

        public override object? ProvideValue(IServiceProvider provider)
        {
            if (!TryGetTargetItems(provider, out var dependencyObject, out _) || !ControlBindLogic.FindDataContext(dependencyObject, out var model)) return null;
            
            Path = Path != null ? new PropertyPath("Value." + Path.Path, Path.PathParameters) : new PropertyPath("Value");
            Source = new DeferredSource(_name, model);
            Binding.Delay = 1000;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding.ValidatesOnNotifyDataErrors = true;

            return base.ProvideValue(provider);
        }
    }
}