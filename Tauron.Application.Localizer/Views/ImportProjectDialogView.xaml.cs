using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ImportProjectDialogView.xaml
    /// </summary>
    public partial class ImportProjectDialogView : IImportProjectDialog
    {
        public ImportProjectDialogView()
        {
            InitializeComponent();
        }

        public Task<ImportProjectDialogResult?> Init(IEnumerable<string> initalData)
        {
            var result = new TaskCompletionSource<ImportProjectDialogResult?>();

            DataContext = new ImportProjectViewModel(s => result.SetResult(s == null ? null : new ImportProjectDialogResult(s)), initalData);

            return result.Task;
        }
    }

    public sealed class ImportProjectViewModel : ObservableObject
    {
        private string? _curretElement;

        public ImportProjectViewModel(Action<string?> selector, IEnumerable<string> projects)
        {
            Projects = projects;

            CancelCommand = new SimpleCommand(() => selector(null));
            SelectCommand = new SimpleCommand(o => !string.IsNullOrWhiteSpace(CurretElement), o => selector(CurretElement));
        }

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
    }
}