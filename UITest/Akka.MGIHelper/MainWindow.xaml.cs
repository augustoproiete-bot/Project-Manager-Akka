using System;
using System.Windows;
using Akka.MGIHelper.Core.Configuration;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;

namespace Akka.MGIHelper
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private readonly IDisposable _setBlocker;
        private readonly WindowOptions _windowOptions;

        public MainWindow(IViewModel<MainWindowViewModel> model, WindowOptions windowOptions)
            : base(model)
        {
            _setBlocker = windowOptions.BlockSet();

            _windowOptions = windowOptions;
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public Window Window => this;

        public event EventHandler? Shutdown;

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            Shutdown?.Invoke(sender, e);
        }

        private void MainWindow_OnLocationChanged(object? sender, EventArgs e)
        {
            _windowOptions.PositionY = Top;
            _windowOptions.PositionX = Left;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = _windowOptions.PositionX;
            Top = _windowOptions.PositionY;
            _setBlocker.Dispose();
        }
    }
}