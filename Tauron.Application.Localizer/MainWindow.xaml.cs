using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Akka.Actor;
using Serilog;
using Syncfusion.SfSkinManager;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.Dialogs;
using Window = System.Windows.Window;

namespace Tauron.Application.Localizer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        private readonly IDialogCoordinator _coordinator;
        private readonly LocLocalizer _localizer;
        private readonly IMainWindowCoordinator _mainWindowCoordinator;
        private readonly ProjectFileWorkspace _workspace;

        public MainWindow(IViewModel<MainWindowViewModel> model, LocLocalizer localizer, IMainWindowCoordinator mainWindowCoordinator, IDialogCoordinator coordinator, ProjectFileWorkspace workspace)
            : base(model)
        {
            _localizer = localizer;
            _mainWindowCoordinator = mainWindowCoordinator;
            _coordinator = coordinator;
            _workspace = workspace;
            InitializeComponent();

            _mainWindowCoordinator.TitleChanged += () => Dispatcher.BeginInvoke(new Action(MainWindowCoordinatorOnTitleChanged));
            _mainWindowCoordinator.IsBusyChanged += IsBusyChanged;

            Closing += OnClosing;
            Closed += async (sender, args) =>
            {
                Shutdown?.Invoke(this, EventArgs.Empty);

                await Task.Delay(TimeSpan.FromSeconds(60));
                Process.GetCurrentProcess().Kill(false);
            };

            _coordinator.OnWindowConstructed
                += window =>
                   {
                       SfSkinManager.SetVisualStyle(window, VisualStyles.Blend);
                   };
        }

        public Window Window => this;

        public event EventHandler? Shutdown;

        private void IsBusyChanged()
        {
            Dispatcher.Invoke(() => SfBusyIndicator.IsBusy = _mainWindowCoordinator.IsBusy);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_mainWindowCoordinator.Saved) return;

            if (_coordinator.ShowModalMessageWindow(_localizer.CommonWarnig, _localizer.MainWindowCloseWarning) == false)
            {
                e.Cancel = true;
                return;
            }

            if (!_workspace.ProjectFile.IsEmpty)
                _workspace.ProjectFile.Operator.Tell(ForceSave.Seal(_workspace.ProjectFile), ActorRefs.NoSender);
        }

        private void MainWindowCoordinatorOnTitleChanged()
        {
            Title = _localizer.MainWindowTitle + " - " + _mainWindowCoordinator.TitlePostfix;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DockingManager.LoadDockState();
            }
            catch (Exception exception)
            {
                Log.ForContext<MainWindow>().Error(exception, "Error on Load Dock State");
                DockingManager.ResetState();
            }
        }

        private void DockReset(object sender, RoutedEventArgs e) 
            => DockingManager.ResetState();
    }
}