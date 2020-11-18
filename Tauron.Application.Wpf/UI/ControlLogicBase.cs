using System;
using System.Windows;
using System.Windows.Input;
using Functional.Maybe;
using JetBrains.Annotations;
using Serilog;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using Tauron.Host;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public abstract class ControlLogicBase<TControl> : IView
        where TControl : FrameworkElement, IView
    {
        protected readonly ControlBindLogic BindLogic;

        protected readonly ILogger    Logger;
        protected readonly IViewModel Model;
        protected readonly TControl   UserControl;

        protected ControlLogicBase(TControl userControl, IViewModel model)
        {
            Logger = Log.ForContext(GetType());

            UserControl             = userControl;
            UserControl.DataContext = model;
            Model                   = model;
            BindLogic               = new ControlBindLogic(userControl, model, Logger);

            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpLoaded();
            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpUnloaded();

            userControl.DataContextChanged += (sender, args) =>
                                              {
                                                  Logger.Debug("DataContext Changed Revert");
                                                  if (args.NewValue != model)
                                                      ((FrameworkElement) sender).DataContext = model;
                                              };
        }

        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            BindLogic.Register(key, bindable, affectedPart);
            CommandManager.InvalidateRequerySuggested();
        }

        public void CleanUp(string key) => BindLogic.CleanUp(key);

        public string      Key         { get; } = Guid.NewGuid().ToString();
        public ViewManager ViewManager => ViewManager.Manager;

        public event Action? ControlUnload;

        protected virtual void WireUpLoaded() => UserControl.Loaded += (_, _) => UserControlOnLoaded();

        protected virtual void WireUpUnloaded() => UserControl.Unloaded += (_, _) => UserControlOnUnloaded();

        protected virtual void UserControlOnUnloaded()
        {
            try
            {
                Logger.Debug("Control Unloaded {Element}", UserControl.GetType());
                BindLogic.CleanUp();
                Tell(Model.Actor, new UnloadEvent(UserControl.Key));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error On Unload User Control");
            }
        }

        protected virtual void UserControlOnLoaded()
        {
            Logger.Debug("Control Loaded {Element}", UserControl.GetType());
            ControlUnload?.Invoke();

            if (!Model.IsInitialized)
            {
                var mayParent = ControlBindLogic.FindParentDatacontext(May(UserControl).Cast<TControl, DependencyObject>());

                Match(mayParent,
                    parent => Tell(parent.Actor, new InitParentViewModel(Model)),
                    () => ViewModelSuperviser.Get(ActorApplication.Application.ActorSystem)
                        .Create(Model));
            }

            Model.AwaitInit(() =>
                            {
                                Tell(Model.Actor, new InitEvent(UserControl.Key));
                                CommandManager.InvalidateRequerySuggested();
                            });
        }
    }
}