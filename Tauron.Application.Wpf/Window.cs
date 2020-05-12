using System;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public class Window : System.Windows.Window, IView
    {
        private sealed class WindowLogic : IDisposable
        {
            private readonly IViewModel _model;
            private readonly Window _window;

            public WindowLogic(IViewModel model, Window window)
            {
                _model = model;
                _window = window;
                window.Loaded += WindowOnLoaded;
            }

            private void WindowOnLoaded(object sender, RoutedEventArgs e)
            {
                if (!_model.IsInitialized)
                    _model.Init();
                _model.Tell(new InitEvent(_window.Key));
                CommandManager.InvalidateRequerySuggested();
            }

            public void Dispose()
            {
                _model.Tell(new UnloadEvent(_window.Key));
                _window.Loaded -= WindowOnLoaded;
            }
        }

        private readonly ControlBindLogic _controlLogic;
        private readonly WindowLogic _windowLogic;

        protected Window(IViewModel viewModel)
        {
            DataContext = viewModel;
            _windowLogic = new WindowLogic(viewModel, this);

            SizeToContent = SizeToContent.Manual;
            ShowInTaskbar = true;
            ResizeMode = ResizeMode.CanResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _controlLogic = new ControlBindLogic(this, viewModel);
            DataContextChanged += (sender, args) =>
                                  {
                                      if (args.NewValue != viewModel)
                                          ((FrameworkElement) sender).DataContext = viewModel;
                                  };
        }

        void IBinderControllable.Register(string key, IControlBindable bindable, DependencyObject affectedPart) 
            => _controlLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) 
            => _controlLogic.CleanUp(key);

        protected override void OnClosed(EventArgs e)
        {
            ControlUnload?.Invoke();
            _windowLogic.Dispose();
            _controlLogic.CleanUp();
            base.OnClosed(e);
        }

        public string Key { get; } = Guid.NewGuid().ToString();
        public ViewManager ViewManager => ViewManager.Manager;

        public event Action? ControlUnload;
    }
}