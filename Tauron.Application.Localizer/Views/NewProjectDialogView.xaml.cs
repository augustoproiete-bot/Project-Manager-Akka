using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Akka.Actor;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    public sealed class NewProjectDialogViewModel : ObservableObject
    {
        private readonly LocLocalizer _localizer;
        private readonly string[] _blocked;
        private string _content = string.Empty;
        private string? _error;

        public NewProjectDialogViewModel(IEnumerable<string> blocked, Action returnAction, IActorRef target, LocLocalizer localizer)
        {
            _localizer = localizer;
            _blocked = blocked.ToArray();

            Return = new SimpleCommand(execute: o =>
            {
                returnAction();
                target.Tell(new NewProjectDialogResult(Content));
            }, canExecute:o => string.IsNullOrWhiteSpace(Error));
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
                if (_blocked.Contains(value))
                    Error = _localizer.NewProjectDialogViewErrorDuplicate;
                OnPropertyChanged();
            }
        }

        public ICommand Return { get; }
    }

    /// <summary>
    /// Interaktionslogik für NewProjectDialogView.xaml
    /// </summary>
    public partial class NewProjectDialogView : IProjectNameDialog
    {
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _coordinator;

        public NewProjectDialogView(LocLocalizer localizer, IDialogCoordinator coordinator)
        {
            _localizer = localizer;
            _coordinator = coordinator;
            InitializeComponent();
        }

        public BaseMetroDialog Dialog => this;
        public void Init(IEnumerable<string> projects, IActorRef resultResponder) 
            => DataContext = new NewProjectDialogViewModel(projects, ReturnAction, resultResponder, _localizer);

        private async void ReturnAction() 
            => await _coordinator.HideMetroDialogAsync("MainWindow", this);

        private void OpenFileDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, NameBox);
            Keyboard.Focus(NameBox);
        }
    }
}
