using System.Windows;
using System.Windows.Threading;
using Autofac;
using Syncfusion.Windows.Tools.Controls;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.ServiceManager.Views;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        private DockingManager _dockingManager = null!;

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            NodeView = this.RegisterModel<NodeViewModel>(nameof(NodeView), "Node-View");
            SeedView = this.RegisterModel<SeedNodeViewModel>(nameof(SeedView), "Seed-View");
            HostView = this.RegisterModel<HostViewModel>(nameof(HostView), "Host-View");
            ApplicationsView = this.RegisterModel<ApplicationsViewModel>(nameof(ApplicationsView), "Applications-View");

            AppInfo = this.RegisterImport<CommonAppInfo>(nameof(AppInfo));

            Receive<DisplayApplications>(_ =>
            {
                UICall(() => _dockingManager.ActivateWindow("ApplicationsView"));
            });
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(DisplayApplications));
            base.PreStart();
        }

        protected override void SetControl(string name, FrameworkElement element)
        {
            switch (name)
            {
                case "DockingManager":
                    _dockingManager = (DockingManager) element;
                    break;
            }
        }

        public ModelProeprty ApplicationsView { get; }

        public ModelProeprty HostView { get; }

        public ModelProeprty NodeView { get; }

        public ModelProeprty SeedView { get; }

        public UIProperty<CommonAppInfo> AppInfo { get; }
    }
}