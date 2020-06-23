using System.Windows.Media;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager.Views
{
    /// <summary>
    /// Interaktionslogik für SeedNodeView.xaml
    /// </summary>
    public partial class SeedNodeView
    {
        public SeedNodeView(IViewModel<SeedNodeViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}
