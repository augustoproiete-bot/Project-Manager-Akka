using System.Windows.Media;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager.Views
{
    /// <summary>
    /// Interaktionslogik für NodeView.xaml
    /// </summary>
    public partial class NodeView
    {
        public NodeView(IViewModel<NodeViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}
