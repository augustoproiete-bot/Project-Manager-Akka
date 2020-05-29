using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Tauron.Application.Localizer.Core.UI;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
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
        private readonly IDialogCoordinator _coordinator;

        public MainWindow(IViewModel<MainWindowViewModel> model, LocLocalizer localizer, IMainWindowCoordinator mainWindowCoordinator, IDialogCoordinator coordinator)
            : base(model)
        {
            _localizer = localizer;
            _mainWindowCoordinator = mainWindowCoordinator;
            _coordinator = coordinator;
            InitializeComponent();

            _mainWindowCoordinator.TitleChanged += () => Dispatcher.BeginInvoke(new Action(MainWindowCoordinatorOnTitleChanged));
            _mainWindowCoordinator.IsBusyChanged += IsBusyChanged;

            Closing += OnClosing;
            Closed += (sender, args) => Shutdown?.Invoke(this, EventArgs.Empty);

            DialogCoordinator.HideDialogEvent += () =>
                                                 {
                                                     if (DialogContent.Content is FrameworkElement element)
                                                     {
                                                         if (element.TryFindResource("Storyboard.Dialogs.Close") is Storyboard story)
                                                         {
                                                             story.Begin(element);

                                                             Task.Delay(200).ContinueWith(t => Dispatcher.Invoke(() =>
                                                             {
                                                                 DialogContent.HideAdorner();
                                                                 DialogContent.Content = null;
                                                             }));

                                                             return;
                                                         }
                                                     }

                                                     DialogContent.HideAdorner();
                                                     DialogContent.Content = null;
                                                 };

            DialogCoordinator.ShowDialogEvent += o =>
                                                 {
                                                     DialogContent.AdornerContent = o as FrameworkElement;
                                                     DialogContent.ShowAdorner();
                                                 };
        }

        private void IsBusyChanged()
        {
            Dispatcher.Invoke(() => SfBusyIndicator.IsBusy = _mainWindowCoordinator.IsBusy);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if(_mainWindowCoordinator.Saved) return;

            if (_coordinator.ShowModalMessageWindow(_localizer.CommonWarnig, _localizer.MainWindowCloseWarning) == true)
                e.Cancel = true;
        }

        private void MainWindowCoordinatorOnTitleChanged() 
            => Title = _localizer.MainWindowTitle + " - " + _mainWindowCoordinator.TitlePostfix;

        public Window Window => this;

        public event EventHandler? Shutdown;
    }
}
