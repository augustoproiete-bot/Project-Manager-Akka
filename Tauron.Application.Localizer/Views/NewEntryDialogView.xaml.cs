using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für NewEntryDialogView.xaml
    /// </summary>
    public partial class NewEntryDialogView : INewEntryDialog
    {
        private readonly LocLocalizer _localizer;

        public NewEntryDialogView(LocLocalizer localizer)
        {
            _localizer = localizer;
            InitializeComponent();
        }

        public Task<NewEntryDialogResult?> Init(IEnumerable<string> initalData)
        {
            var result = new TaskCompletionSource<NewEntryDialogResult?>();

            DataContext = new NewEntryDialogViewModel(s => result.SetResult(string.IsNullOrWhiteSpace(s) ? null : new NewEntryDialogResult(s)), initalData, _localizer);

            return result.Task;
        }
    }

    public sealed class NewEntryDialogViewModel : ObservableObject
    {
        private readonly LocLocalizer _localizer;
        private readonly string[] _entrys;

        private string _content = string.Empty;
        private string _error = string.Empty;

        public NewEntryDialogViewModel(Action<string?> selector, IEnumerable<string> entrys, LocLocalizer localizer)
        {
            _localizer = localizer;
            _entrys = entrys.ToArray();
            Return = new SimpleCommand(() => string.IsNullOrWhiteSpace(Error),() => selector(Content));
        }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
                Error = _entrys.Contains(value) ? _localizer.NewEntryDialogViewDuplicateError : string.Empty;
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public ICommand Return { get; }
    }
}
