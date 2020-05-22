using System;
using System.Collections.Generic;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Xaml.Behaviors.Core;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für ImportProjectDialogView.xaml
    /// </summary>
    public partial class ImportProjectDialogView : IImportProjectDialog
    {
        private readonly IDialogCoordinator _coordinator;

        public ImportProjectDialogView(IDialogCoordinator coordinator)
        {
            _coordinator = coordinator;
            InitializeComponent();
        }

        public void Init(Action<string?> selector, IEnumerable<string> projects) 
            => DataContext = new ImportProjectViewModel(async s =>
                                                        {
                                                            await _coordinator.HideMetroDialogAsync("MainWindow", this);
                                                            selector(s);
                                                        }, projects);

        public BaseMetroDialog Dialog => this;
    }

    public sealed class ImportProjectViewModel : ObservableObject
    {
        private string? _curretElement;

        public IEnumerable<string> Projects { get; }

        public string? CurretElement
        {
            get => _curretElement;
            set
            {
                if (value == _curretElement) return;
                _curretElement = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectCommand { get; }

        public ICommand CancelCommand { get; }

        public ImportProjectViewModel(Action<string?> selector, IEnumerable<string> projects)
        {
            Projects = projects;

            CancelCommand = new ActionCommand(() => selector(null));
            SelectCommand = new SimpleCommand(o => !string.IsNullOrWhiteSpace(CurretElement), o => selector(CurretElement));
        }
    }
}
