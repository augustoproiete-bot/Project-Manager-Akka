using System.Threading.Tasks;
using System.Windows.Input;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.ServiceManager.Views.SetupDialogs
{
    /// <summary>
    /// Interaktionslogik für InitialDialog.xaml
    /// </summary>
    public partial class InitialDialog : IInitialDialog
    {
        public InitialDialog()
        {
            InitializeComponent();
        }

        public Task<string?> Init(string? initalData)
        {
            return MakeTask<string?>(s => new InitialDialogModel(s, LocLocalizer.Inst));
        }
    }

    public sealed class InitialDialogModel : ObservableObject
    {
        public InitialDialogModel(TaskCompletionSource<string?> watch, LocLocalizer localizer)
        {
            Title = localizer.InitialDialog.Title;
            BackText = localizer.Common.Back;
            NextText = localizer.Common.Next;
            MainText = localizer.InitialDialog.MainTextOne;

            Back = new SimpleCommand(_ => watch.SetResult(null));
            Next = new SimpleCommand(_ =>
            {
                if (_firstPage)
                {
                    _firstPage = false;
                    MainText = localizer.InitialDialog.MainTextTwo;
                }
                else
                    watch.SetResult("Setup");
            });
        }

        private bool _firstPage = true;
        private string _mainText;

        public string MainText
        {
            get => _mainText;
            set
            {
                if (value == _mainText) return;
                _mainText = value;
                OnPropertyChanged();
            }
        }

        public string Title { get; }

        public string BackText { get; }

        public string NextText { get; }

        public ICommand Next { get; }

        public ICommand Back { get; }
    }
}
