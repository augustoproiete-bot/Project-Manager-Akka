using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    public sealed class NewProjectDialogViewModel : ObservableObject
    {
        private readonly string[] _blocked;
        private readonly LocLocalizer _localizer;
        private string _content = string.Empty;
        private string? _error;

        public NewProjectDialogViewModel(IEnumerable<string> blocked, Action<NewProjectDialogResult> target, LocLocalizer localizer)
        {
            _localizer = localizer;
            _blocked = blocked.ToArray();

            Return = new SimpleCommand(execute: o => target(new NewProjectDialogResult(Content)), canExecute: o => string.IsNullOrWhiteSpace(Error));
        }

        public string? Error
        {
            get => _error;
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                Error = _blocked.Contains(value) ? _localizer.NewProjectDialogViewErrorDuplicate : null;
                OnPropertyChanged();
            }
        }

        public ICommand Return { get; }
    }

    /// <summary>
    ///     Interaktionslogik für NewProjectDialogView.xaml
    /// </summary>
    public partial class NewProjectDialogView : IProjectNameDialog
    {
        private readonly LocLocalizer _localizer;

        public NewProjectDialogView(LocLocalizer localizer)
        {
            _localizer = localizer;
            InitializeComponent();
        }

        public Task<NewProjectDialogResult> Init(IEnumerable<string> initalData)
        {
            var task = new TaskCompletionSource<NewProjectDialogResult>();
            DataContext = new NewProjectDialogViewModel(initalData, result => task.SetResult(result), _localizer);
            return task.Task;
        }

        private void OpenFileDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, NameBox);
            Keyboard.Focus(NameBox);
        }
    }
}