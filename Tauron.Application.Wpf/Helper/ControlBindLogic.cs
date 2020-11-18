using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Functional.Maybe;
using JetBrains.Annotations;
using Serilog;
using static Tauron.Prelude;
// ReSharper disable SuspiciousTypeConversion.Global

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

        private RootedDataContextPromise(FrameworkElement element) => _element = element;

        public static DataContextPromise CreateFrom(FrameworkElement element)
            => new RootedDataContextPromise(element);

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

        private DisconnectedDataContextRoot(Maybe<FrameworkElement> mayElementBase)
        {
            var mayRoot = ControlBindLogic.FindRoot(mayElementBase.Cast<FrameworkElement, DependencyObject>());

            var elementBase = mayElementBase.Value;

            void OnLoad(object sender, RoutedEventArgs args)
            {
                elementBase.Loaded -= OnLoad;

                var root = mayRoot.OrElseDefault();

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

        public static DataContextPromise CreateFrom(FrameworkElement element) => new DisconnectedDataContextRoot(May(element));
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

        public static Maybe<IBinderControllable> FindRoot(Maybe<DependencyObject> mayAffected) 
            => TryFindGeneric("Root", mayAffected, o => MayNotNull(o as IBinderControllable));

        public static Maybe<IView> FindParentView(Maybe<DependencyObject> affected) 
            => TryFindGeneric("View", affected, o => MayNotNull(o as IView));

        public static Maybe<IViewModel> FindParentDatacontext(Maybe<DependencyObject> mayAffected) 
            => TryFindGeneric("DataContext", mayAffected, o => MayNotNull(() => o is FrameworkElement {DataContext: IViewModel model} ? model : null));

        private static Maybe<TResult> TryFindGeneric<TResult>(string searchName, Maybe<DependencyObject> mayAffected, Func<DependencyObject, Maybe<TResult>> convert)
        {
            var affected = mayAffected.OrElseDefault();

            Log.Debug($"Try Find {searchName} for" + "{Element}", affected?.GetType());

            while (affected != null)
            {
                var result = convert(affected);
                if (result.IsSomething())
                {
                    Log.Debug($"{searchName} Found for" + "{Element}", affected.GetType());
                    return result;
                }

                affected = LogicalTreeHelper.GetParent(affected) ?? VisualTreeHelper.GetParent(affected);
            }

            Log.Debug($"{searchName} Not Found for" + "{Element}", affected?.GetType());
            return Maybe<TResult>.Nothing;
        }

        public static Maybe<DataContextPromise> FindDataContext(Maybe<DependencyObject> mayAffected)
        {
            var tryRoot =
                from root in FindRoot(mayAffected)
                where root is FrameworkElement
                select RootedDataContextPromise.CreateFrom((FrameworkElement) root);

            return tryRoot.Or(() =>
                from affected in mayAffected
                where affected is FrameworkElement
                select DisconnectedDataContextRoot.CreateFrom((FrameworkElement) affected));
        }

        public static void MakeLazy(FrameworkElement target, Maybe<string> newValue, Maybe<string> oldValue, LazyRunner runner)
        {
            var temp = new LazyHelper(target, newValue, oldValue, runner);
            target.Loaded += temp.ElementOnLoaded;
        }

        public delegate void LazyRunner(Maybe<string> oldValue, Maybe<string> newValue, IBinderControllable target, DependencyObject dependencyObject);

        private class LazyHelper
        {
            private readonly Maybe<string> _newValue;
            private readonly Maybe<string> _oldValue;
            private readonly LazyRunner _runner;
            private readonly FrameworkElement _target;

            public LazyHelper(FrameworkElement target, Maybe<string> newValue, Maybe<string> oldValue, LazyRunner runner)
            {
                _target = target;
                _newValue = newValue;
                _oldValue = oldValue;
                _runner = runner;
            }

            public void ElementOnLoaded(object sender, RoutedEventArgs e)
            {
                Finally(() =>
                    from root in FindRoot(May<DependencyObject>(_target)) 
                    select Action(() => _runner(_oldValue, _newValue, root, _target)),
                    () => _target.Loaded -= ElementOnLoaded);
            }
        }
    }
}