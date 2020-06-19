using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
