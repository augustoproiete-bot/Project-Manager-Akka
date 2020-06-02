using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class UpdatableMarkupExtension : MarkupExtension
    {
        protected object? TargetObject { get; private set; }

        protected object? TargetProperty { get; private set; }

        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)
            {
                if (service.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                    return this;

                TargetObject = service.TargetObject;
                TargetProperty = service.TargetProperty;
            }

            var isDesign = TargetObject is DependencyObject dependencyObject && DesignerProperties.GetIsInDesignMode(dependencyObject);

            return isDesign ? DesignTime() : ProvideValueInternal(serviceProvider);
        }

        protected void UpdateValue(object? value)
        {
            if (TargetObject != null)
            {
                if (TargetProperty is DependencyProperty dependencyProperty)
                {
                    var obj = TargetObject as DependencyObject;

                    void UpdateAction()
                    {
                        obj.SetValue(dependencyProperty, value);
                    }

                    // Check whether the target object can be accessed from the
                    // current thread, and use Dispatcher.Invoke if it can't

                    if (obj?.CheckAccess() == true)
                        UpdateAction();
                    else
                        obj?.Dispatcher.BeginInvoke(new Action(UpdateAction), DispatcherPriority.Background);
                }
                else // _targetProperty is PropertyInfo
                {
                    var prop = TargetProperty as PropertyInfo;
                    prop?.SetValue(TargetObject, value, null);
                }
            }
        }

        protected virtual bool TryGetTargetItems(IServiceProvider? provider, [NotNullWhen(true)] out DependencyObject? target, [NotNullWhen(true)] out DependencyProperty? dp)
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