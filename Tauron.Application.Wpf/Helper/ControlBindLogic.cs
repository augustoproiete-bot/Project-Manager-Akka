using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using Serilog;

namespace Tauron.Application.Wpf.Helper
{
    public abstract class DataContextPromise
    {
        public abstract void OnUnload(Action unload);

        public abstract void OnContext(Action<IViewModel, IView> modelAction);

        public abstract void OnNoContext(Action action);
    }

    public sealed class RootedDataContextPromise : DataContextPromise
    {
        private readonly FrameworkElement _element;

        private Action? _noContext;

        public RootedDataContextPromise(FrameworkElement element) => _element = element;

        public override void OnUnload(Action unload)
        {
            if (_element is IView view)
                view.ControlUnload += unload;
        }

        public override void OnContext(Action<IViewModel, IView> modelAction)
        {
            if (_element is IView view)
            {
                if (_element.DataContext is IViewModel model)
                {
                    modelAction(model, view);
                    return;
                }

                void OnElementOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
                {
                    if (args.NewValue is IViewModel localModel)
                        modelAction(localModel, view);
                    else
                        _noContext?.Invoke();

                    _element.DataContextChanged -= OnElementOnDataContextChanged;
                }

                _element.DataContextChanged += OnElementOnDataContextChanged;
            }
            else
                _noContext?.Invoke();
        }

        public override void OnNoContext(Action action) => _noContext = action;
    }

    public sealed class DisconnectedDataContextRoot : DataContextPromise
    {
        private Action<IViewModel, IView>? _modelAction;
        private Action?                    _noContext;
        private Action?                    _unload;

        public DisconnectedDataContextRoot(FrameworkElement elementBase)
        {
            var root = ControlBindLogic.FindRoot(elementBase);

            void OnLoad(object sender, RoutedEventArgs args)
            {
                elementBase.Loaded -= OnLoad;

                if (root is IView control && root is FrameworkElement {DataContext: IViewModel model})
                {
                    _modelAction?.Invoke(model, control);

                    if (_unload != null)
                        control.ControlUnload += _unload;
                }
                else
                    _noContext?.Invoke();
            }

            elementBase.Loaded += OnLoad;
        }

        public override void OnUnload(Action unload) => _unload = unload;

        public override void OnContext(Action<IViewModel, IView> modelAction) => _modelAction = modelAction;

        public override void OnNoContext(Action noContext) => _noContext = noContext;
    }

    [PublicAPI]
    public sealed class ControlBindLogic
    {
        private readonly Dictionary<string, (IDisposable Disposer, IControlBindable Binder)> _binderList = new();
        private readonly object?                                                             _dataContext;
        private readonly ILogger                                                             _log;

        private readonly DependencyObject _target;

        public ControlBindLogic(DependencyObject target, object? dataContext, ILogger log)
        {
            _target      = target;
            _dataContext = dataContext;
            _log         = log;
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
            _log.Debug("Clean Up Bind Control");

            foreach (var pair in _binderList)
                pair.Value.Disposer.Dispose();

            _binderList.Clear();
        }

        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            Log.Debug("Register Bind Element {Name} -- {LinkElement} -- {Part}", key, bindable.GetType(), affectedPart.GetType());

            if (_dataContext == null)
                return;

            var disposer = bindable.Bind(_target, affectedPart, _dataContext);

            if (affectedPart is FrameworkElement element)
            {
                void OnElementOnUnloaded(object sender, RoutedEventArgs args)
                {
                    disposer.Dispose();
                    _binderList.Remove(key);
                    element.Unloaded -= OnElementOnUnloaded;
                }

                element.Unloaded += OnElementOnUnloaded;
            }

            _binderList[key] = (disposer, bindable);
        }

        public void CleanUp(string key)
        {
            Log.Debug("Clean Up Element {Name}", key);

            if (_binderList.TryGetValue(key, out var pair))
                pair.Disposer.Dispose();

            _binderList.Remove(key);
        }

        public static IBinderControllable? FindRoot(DependencyObject? affected)
        {
            
            Log.Debug("Try Find Root for {Element}", affected?.GetType());

            while (affected != null)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (affected is IBinderControllable binder)
                {
                    Log.Debug("Root Found for {Element}", affected.GetType());
                    return binder;
                }

                affected = LogicalTreeHelper.GetParent(affected) ?? VisualTreeHelper.GetParent(affected);
            }

            Log.Debug("Root not Found for {Element}", affected?.GetType());
            return null;
        }

        public static IView? FindParentView(DependencyObject? affected)
        {
            Log.Debug("Try Find View for {Element}", affected?.GetType());
            while(affected != null)
            {
                affected = LogicalTreeHelper.GetParent(affected) ?? VisualTreeHelper.GetParent(affected);
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (!(affected is IView binder)) continue;

                Log.Debug("View Found for {Element}", affected.GetType());
                return binder;
            }

            Log.Debug("View Not Found for {Element}", affected?.GetType());
            return null;
        }

        public static IViewModel? FindParentDatacontext(DependencyObject? affected)
        {
            Log.Debug("Try Find DataContext for {Element}", affected?.GetType());

            while (affected != null)
            {
                affected = LogicalTreeHelper.GetParent(affected) ?? VisualTreeHelper.GetParent(affected);
                
                if (affected is not FrameworkElement element || element.DataContext is not IViewModel model) continue;
                
                Log.Debug("DataContext Found for {Element}", affected.GetType());
                return model;
            }

            Log.Debug("DataContext Not Found for {Element}", affected?.GetType());
            return null;
        }

        public static bool FindDataContext(DependencyObject? affected, [NotNullWhen(true)] out DataContextPromise? promise)
        {
            promise = null;
            var root = FindRoot(affected);
            if (root is FrameworkElement element)
                promise = new RootedDataContextPromise(element);
            else if (affected is FrameworkElement affectedElement)
                promise = new DisconnectedDataContextRoot(affectedElement);


            return promise != null;
        }

        public static void MakeLazy(FrameworkElement target, string? newValue, string? oldValue, Action<string?, string?, IBinderControllable, DependencyObject> runner)
        {
            var temp = new LazyHelper(target, newValue, oldValue, runner);
            target.Loaded += temp.ElementOnLoaded;
        }

        private class LazyHelper
        {
            private readonly string?                                                         _newValue;
            private readonly string?                                                         _oldValue;
            private readonly Action<string?, string?, IBinderControllable, DependencyObject> _runner;
            private readonly FrameworkElement                                                _target;

            public LazyHelper(FrameworkElement target, string? newValue, string? oldValue, Action<string?, string?, IBinderControllable, DependencyObject> runner)
            {
                _target   = target;
                _newValue = newValue;
                _oldValue = oldValue;
                _runner   = runner;
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