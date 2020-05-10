using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class UpdatableMarkupExtension : MarkupExtension
    {
        protected DependencyObject? TargetObject { get; private set; }

        protected DependencyProperty? TargetProperty { get; private set; }

        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (TryGetTargetItems(serviceProvider, out var dependencyObject, out var dependencyProperty))
            {
                TargetObject = dependencyObject;
                TargetProperty = dependencyProperty;
            }

            return DesignerProperties.GetIsInDesignMode(TargetObject) ? DesignTime() : ProvideValueInternal(serviceProvider);
        }

        protected void UpdateValue(object? value)
        {
            if (TargetObject == null || TargetProperty == null) return;
            
            void UpdateAction() => TargetObject.SetValue(TargetProperty, value);

            // Check whether the target object can be accessed from the
            // current thread, and use Dispatcher.Invoke if it can't

            if (TargetObject.CheckAccess())
                UpdateAction();
            else
                TargetObject.Dispatcher.Invoke(UpdateAction);
        }

        protected virtual bool TryGetTargetItems(IServiceProvider? provider, [NotNullWhen(true)]out DependencyObject? target, [NotNullWhen(true)] out DependencyProperty? dp)
        {
            target = null;
            dp = null;

            //create a binding and assign it to the target
            if (!(provider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)) return false;

            //we need dependency objects / properties
            target = service.TargetObject as DependencyObject;
            dp = service.TargetProperty as DependencyProperty;
            return target != null && dp != null;
        }

        protected abstract object DesignTime();

        protected abstract object ProvideValueInternal(IServiceProvider serviceProvider);
    }
}