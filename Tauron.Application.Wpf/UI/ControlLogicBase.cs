using System;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class ControlLogicBase<TControl> : IView
        where TControl : FrameworkElement, IView
    {
        protected readonly TControl UserControl;
        protected readonly IViewModel Model;
        protected readonly ControlBindLogic BindLogic;

        protected ControlLogicBase(TControl userControl, IViewModel model)
        {
            UserControl = userControl;
            UserControl.DataContext = model;
            Model = model;
            BindLogic = new ControlBindLogic(userControl, model);

            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpLoaded();
            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpUnloaded();

            userControl.DataContextChanged += (sender, args) =>
            {
                if (args.NewValue != model)
                    ((FrameworkElement)sender).DataContext = model;
            };

        }

        protected virtual void WireUpLoaded()
        {
            UserControl.Loaded += (sender, args) =>  UserControlOnLoaded();
        }

        protected virtual void WireUpUnloaded()
        {
            UserControl.Unloaded += (sender, args) =>  UserControlOnUnloaded();
        }

        protected virtual void UserControlOnUnloaded()
        {
            BindLogic.CleanUp();
            Model.Tell(new UnloadEvent(UserControl.Key));
        }

        protected virtual void UserControlOnLoaded()
        {
            ControlUnload?.Invoke();

            if (!Model.IsInitialized)
            {
                var parent = ControlBindLogic.FindParentDatacontext(UserControl);
                if (parent != null)
                    parent.Tell(new InitParentViewModel(Model));
                else
                    Model.Init();
            }

            Model.Tell(new InitEvent(((IView)UserControl).Key));
            CommandManager.InvalidateRequerySuggested();
        }

        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            BindLogic.Register(key, bindable, affectedPart);
            CommandManager.InvalidateRequerySuggested();
        }

        public void CleanUp(string key)
            => BindLogic.CleanUp(key);

        public string Key { get; } = Guid.NewGuid().ToString();
        public ViewManager ViewManager => ViewManager.Manager;

        public event Action? ControlUnload;
    }
}