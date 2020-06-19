using System.Windows.Threading;
using Autofac;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IViewModel<NodeViewModel> nodeModel) 
            : base(lifetimeScope, dispatcher)
        {
            nodeModel.Init(Context, "Node-View");
            NodeView = RegisterProperty<IViewModel<NodeViewModel>>(nameof(NodeView)).WithDefaultValue(nodeModel);
        }

        public UIProperty<IViewModel<NodeViewModel>> NodeView { get; }
    }
}