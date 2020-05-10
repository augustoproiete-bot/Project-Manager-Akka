using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Helper
{
    public sealed class DataContextPromise
    {
        private readonly FrameworkElement _element;

        public DataContextPromise(FrameworkElement element) 
            => _element = element;

        public void OnContext(Action<IViewModel> modelAction)
        {
            if (_element.DataContext is IViewModel model)
            {
                modelAction(model);
                return;
            }

            void OnElementOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
            {
                if (!(args.NewValue is IViewModel localModel)) return;

                modelAction(localModel);
                _element.DataContextChanged -= OnElementOnDataContextChanged;
            }

            _element.DataContextChanged += OnElementOnDataContextChanged;
        }
    }

    [PublicAPI]
    public sealed class ControlBindLogic
    {
        private readonly Dictionary<string, (IDisposable Disposer, IControlBindable Binder)> _binderList = new Dictionary<string, (IDisposable Disposer, IControlBindable Binder)>();
        private readonly object _dataContext;

        private readonly DependencyObject _target;

        public ControlBindLogic(DependencyObject target, object dataContext)
        {
            _target = target;
            _dataContext = dataContext;
        }

        //public void NewDataContext(object? dataContext)
        //{
        //    _dataContext = dataContext;

        //    foreach (var (key, (disposer, binder)) in _binderList.ToArray())
        //    {
        //        disposer.Dispose();
        //        if (dataContext != null)
        //            _binderList[key] = (binder.NewContext(dataContext), binder);
        //    }
        //}

        public void CleanUp()
        {
            foreach (var pair in _binderList)
                pair.Value.Disposer.Dispose();

            _binderList.Clear();
        }

        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            if (_dataContext == null)
                return;

            var disposer = bindable.Bind(_target, affectedPart, _dataContext);

            _binderList[key] = (disposer, bindable);
        }

        public void CleanUp(string key)
        {
            if (_binderList.TryGetValue(key, out var pair))
                pair.Disposer.Dispose();

            _binderList.Remove(key);
        }

        public static IBinderControllable? FindRoot(DependencyObject? affected)
        {
            do
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (affected is IBinderControllable binder)
                    return binder;
                affected = LogicalTreeHelper.GetParent(affected);
            } while (affected != null);

            return null;
        }

        public static IViewModel? FindParentDatacontext(DependencyObject? affected)
        {
            do
            {
                affected = LogicalTreeHelper.GetParent(affected);
                if (affected is FrameworkElement element && element.DataContext is IViewModel model)
                    return model;

            } while (affected != null);

            return null;
        }

        public static bool FindDataContext(DependencyObject? affected, [NotNullWhen(true)]out DataContextPromise? promise)
        {
            promise = null;
            var root = FindRoot(affected);
            if(root is FrameworkElement element)
                promise = new DataContextPromise(element);

            return promise != null;
        }

        public static void MakeLazy(FrameworkElement target, string? newValue, string? oldValue, Action<string?, string?, IBinderControllable, DependencyObject> runner)
        {
            var temp = new LazyHelper(target, newValue, oldValue, runner);
            target.Loaded += temp.ElementOnLoaded;
        }

        private class LazyHelper
        {
            private readonly string? _newValue;
            private readonly string? _oldValue;
            private readonly Action<string?, string?, IBinderControllable, DependencyObject> _runner;
            private readonly FrameworkElement _target;

            public LazyHelper(FrameworkElement target, string? newValue, string? oldValue, Action<string?, string?, IBinderControllable, DependencyObject> runner)
            {
                _target = target;
                _newValue = newValue;
                _oldValue = oldValue;
                _runner = runner;
            }

            public void ElementOnLoaded(object sender, RoutedEventArgs e)
            {
                try
                {
                    var root = FindRoot(_target);
                    if (root == null) return;

                    _runner(_oldValue, _newValue, root, _target);
                }
                finally
                {
                    _target.Loaded -= ElementOnLoaded;
                }
            }
        }
    }
}