using System;
using System.Windows;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public class Window : System.Windows.Window, IView
    {

        private readonly WindowControlLogic _controlLogic;

        protected Window(IViewModel viewModel) => _controlLogic = new WindowControlLogic(this, viewModel);
        public void Register(string key, IControlBindable bindable, DependencyObject affectedPart) => _controlLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) => _controlLogic.CleanUp(key);

        public string Key => _controlLogic.Key;

        public ViewManager ViewManager => _controlLogic.ViewManager;

        public event Action? ControlUnload
        {
            add => _controlLogic.ControlUnload += value;
            remove => _controlLogic.ControlUnload -= value;
        }
    }
}