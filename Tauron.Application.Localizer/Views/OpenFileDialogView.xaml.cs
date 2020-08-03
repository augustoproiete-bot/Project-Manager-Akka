using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für OpenFileDialogView.xaml
    /// </summary>
    public partial class OpenFileDialogView : IOpenFileDialog
    {
        private readonly IDialogFactory _dialogFactory;
        private readonly OpenFileMode _filemode;
        private readonly LocLocalizer _localizer;
        private readonly TaskCompletionSource<string?> _selector = new TaskCompletionSource<string?>();

        public OpenFileDialogView(IDialogFactory dialogFactory, LocLocalizer localizer, OpenFileMode filemode)
        {
            _dialogFactory = dialogFactory;
            _localizer = localizer;
            _filemode = filemode;
            InitializeComponent();

            if (filemode == OpenFileMode.OpenNewFile)
                Title += _localizer.OpenFileDialogViewHeaderNewPrefix;
        }

        public Task<string?> Init(string? initalData)
        {
            return _selector.Task;
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _dialogFactory.ShowOpenFileDialog(null, _filemode == OpenFileMode.OpenExistingFile, "transp", true, _localizer.OpenFileDialogViewDialogFilter, false,
                _localizer.OpenFileDialogViewDialogTitle,
                true, true, out var ok);

            if (ok != true) return;

            PART_Path.Text = result.FirstOrDefault();
        }

        private void Ready_OnClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            string text = PART_Path.Text;
            _selector.SetResult(text);
        }

        private void OpenFileDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, PART_Path);
            Keyboard.Focus(PART_Path);
        }
    }
}