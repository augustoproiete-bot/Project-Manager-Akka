using System;
using Tauron.Application.Localizer.ViewModels;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.AppCore;
using Window = System.Windows.Window;

namespace Tauron.Application.Localizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow
    {
        public MainWindow(IViewModel<MainWindowViewModel> model)
            : base(model)
        {
            InitializeComponent();

            Closed += (sender, args) => Shutdown?.Invoke(this, EventArgs.Empty);
        }

        public Window Window => this;

        public event EventHandler? Shutdown;
    }
}
