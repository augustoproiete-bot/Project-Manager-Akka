using System;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;

namespace Tauron.Application.ServiceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        public MainWindow(IViewModel<MainWindowViewModel> model, LocLocalizer localizer)
            : base(model)
        {
            LocLocalizer.Inst = localizer;

            InitializeComponent();

            Closed += (sender, args) => Shutdown?.Invoke(this, EventArgs.Empty);
        }

        public Window Window => this;
        public event EventHandler? Shutdown;
    }
}
