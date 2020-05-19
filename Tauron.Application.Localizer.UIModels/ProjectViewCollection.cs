using System.Collections.ObjectModel;
using System.Windows.Data;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectViewCollection : ObservableCollection<ProjectViewContainer>
    {
        public ProjectViewCollection() => BindingOperations.EnableCollectionSynchronization(this, new object());
    }
}