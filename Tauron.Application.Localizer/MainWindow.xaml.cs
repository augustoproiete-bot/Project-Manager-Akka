using System;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;

namespace Tauron.Application.Localizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private readonly LocLocalizer _localizer;
        private readonly IMainWindowCoordinator _mainWindowCoordinator;

        public MainWindow(IViewModel<MainWindowViewModel> model, LocLocalizer localizer, IMainWindowCoordinator mainWindowCoordinator)
            : base(model)
        {
            _localizer = localizer;
            _mainWindowCoordinator = mainWindowCoordinator;
            InitializeComponent();

            _mainWindowCoordinator.TitleChanged += () => Dispatcher.BeginInvoke(new Action(MainWindowCoordinatorOnTitleChanged));
            _mainWindowCoordinator.IsBusyChanged += IsBusyChanged;

            Closing += OnClosing;
            Closed += (sender, args) => Shutdown?.Invoke(this, EventArgs.Empty);
        }

        private void IsBusyChanged()
        {
            Dispatcher.Invoke(() =>
            {
                _LoadingIndicator.IsActive = _mainWindowCoordinator.IsBusy;
                _AdornedControl.IsAdornerVisible = _mainWindowCoordinator.IsBusy;
            });
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if(_mainWindowCoordinator.Saved) return;

            if (this.ShowModalMessageExternal(_localizer.CommonWarnig, _localizer.MainWindowCloseWarning, MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Negative)
                e.Cancel = true;
        }

        private void MainWindowCoordinatorOnTitleChanged() 
            => Title = _localizer.MainWindowTitle + " - " + _mainWindowCoordinator.TitlePostfix;

        public Window Window => this;

        public event EventHandler? Shutdown;
    }
}
