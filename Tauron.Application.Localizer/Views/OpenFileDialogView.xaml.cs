using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für OpenFileDialogView.xaml
    /// </summary>
    public partial class OpenFileDialogView : IOpenFileDialog
    {
        private readonly IDialogCoordinator _coordinator;
        private readonly IDialogFactory _dialogFactory;
        private readonly LocLocalizer _localizer;
        private readonly OpenFileMode _filemode;
        private readonly TaskCompletionSource<string?> _selector = new TaskCompletionSource<string?>();

        public OpenFileDialogView(IDialogCoordinator coordinator, IDialogFactory dialogFactory, LocLocalizer localizer, OpenFileMode filemode)
        {
            _coordinator = coordinator;
            _dialogFactory = dialogFactory;
            _localizer = localizer;
            _filemode = filemode;
            InitializeComponent();

            if (filemode == OpenFileMode.OpenNewFile)
                Title += _localizer.OpenFileDialogViewHeaderNewPrefix;
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _dialogFactory.ShowOpenFileDialog(null, _filemode == OpenFileMode.OpenExistingFile, "transp", true, _localizer.OpenFileDialogViewDialogFilter, false,
                                                                            _localizer.OpenFileDialogViewDialogTitle,
                true, true, out bool? ok);

            if(ok != true) return;

            PART_Path.Text = result.FirstOrDefault();
        }

        private async void Ready_OnClick(object sender, RoutedEventArgs e)
        {
            string text = PART_Path.Text;
            await _coordinator.HideMetroDialogAsync(MainWindowViewModel.MainWindow, this);
            _selector.SetResult(text);
        }

        public BaseMetroDialog Dialog => this;

        public Task<string?> Init(IEnumerable<string?> initalData) => _selector.Task;

        private void OpenFileDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, PART_Path);
            Keyboard.Focus(PART_Path);
        }
    }
}
