using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Tauron.Akka;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Localizer.Core.UI
{
    public class InternalMetroWindow : MetroWindow, IView
    {
        private sealed class WindowLogic : IDisposable
        {
            private readonly IViewModel _model;
            private readonly System.Windows.Window _window;

            public WindowLogic(IViewModel model, System.Windows.Window window)
            {
                _model = model;
                _window = window;
                window.Loaded += WindowOnLoaded;
            }

            private void WindowOnLoaded(object sender, RoutedEventArgs e)
            {
                if (!_model.IsInitialized)
                    _model.Init();
                _model.Tell(new InitEvent());
                CommandManager.InvalidateRequerySuggested();
            }

            public void Dispose()
            {
                _model.Tell(new UnloadEvent());
                _window.Loaded -= WindowOnLoaded;
                _model.Reset();
            }
        }

        private readonly ControlBindLogic _controlLogic;
        private readonly WindowLogic _windowLogic;

        protected InternalMetroWindow(IViewModel viewModel)
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
                    ((FrameworkElement)sender).DataContext = viewModel;
            };
        }

        void IBinderControllable.Register(string key, IControlBindable bindable, DependencyObject affectedPart)
            => _controlLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key)
            => _controlLogic.CleanUp(key);

        protected override void OnClosed(EventArgs e)
        {
            _windowLogic.Dispose();
            _controlLogic.CleanUp();
            base.OnClosed(e);
        }
    }
}