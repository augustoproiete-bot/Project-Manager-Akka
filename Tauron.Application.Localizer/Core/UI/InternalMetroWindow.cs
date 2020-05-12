using System;
using System.Windows;
using MahApps.Metro.Controls;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Localizer.Core.UI
{
    public class InternalMetroWindow : MetroWindow, IView
    {
        private sealed class MetroWindowLogic : ControlLogicBase<InternalMetroWindow>
        {
            public MetroWindowLogic(InternalMetroWindow userControl, IViewModel model) : base(userControl, model)
            {
            }

            protected override void WireUpUnloaded()
            {
                UserControl.Closed += (sender, args) => UserControlOnUnloaded();
            }
        }

        private readonly MetroWindowLogic _windowLogic;

        protected InternalMetroWindow(IViewModel viewModel) 
            => _windowLogic = new MetroWindowLogic(this, viewModel);

        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart) 
            => _windowLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) 
            => _windowLogic.CleanUp(key);

        public string Key => _windowLogic.Key;

        public ViewManager ViewManager => _windowLogic.ViewManager;

        public event Action? ControlUnload
        {
            add => _windowLogic.ControlUnload += value;
            remove => _windowLogic.ControlUnload -= value;
        }
    }
}