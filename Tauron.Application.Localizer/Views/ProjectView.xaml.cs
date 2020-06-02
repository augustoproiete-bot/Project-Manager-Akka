using System.Windows.Media;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ProjectView.xaml
    /// </summary>
    public partial class ProjectView
    {
        public ProjectView(IViewModel<ProjectViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}