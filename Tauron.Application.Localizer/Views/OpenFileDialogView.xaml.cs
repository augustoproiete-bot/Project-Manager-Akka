using System;
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
    public partial class OpenFileDialogView : IOpenFileView
    {
        private readonly IDialogCoordinator _coordinator;
        private readonly IDialogFactory _dialogFactory;
        private readonly LocLocalizer _localizer;
        private readonly Action<string?> _result;

        public OpenFileDialogView(IDialogCoordinator coordinator, IDialogFactory dialogFactory, LocLocalizer localizer, Action<string?> result)
        {
            _coordinator = coordinator;
            _dialogFactory = dialogFactory;
            _localizer = localizer;
            _result = result;
            InitializeComponent();
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var result = _dialogFactory.ShowOpenFileDialog(null, true, "transp", true, _localizer.OpenFileDialogViewDialogFilter, false, _localizer.OpenFileDialogViewDialogTitle,
                true, true, out bool? ok);

            if(ok != true) return;

            PART_Path.Text = result.FirstOrDefault();
        }

        private async void Ready_OnClick(object sender, RoutedEventArgs e)
        {
            string text = PART_Path.Text;
            await _coordinator.HideMetroDialogAsync(MainWindowViewModel.MainWindow, this);
            _result(text);
        }

        public BaseMetroDialog Dialog => this;
    }
}
