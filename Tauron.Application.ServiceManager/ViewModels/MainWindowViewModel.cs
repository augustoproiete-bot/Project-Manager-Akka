using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Akka.Event;
using Autofac;
using Syncfusion.Windows.Tools.Controls;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Model;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        private readonly AppConfig _config;
        private DockingManager _dockingManager = null!;

        public ModelProperty SetupBuilderView { get; }

        public ModelProperty ConfigurationView { get; }

        public ModelProperty ApplicationsView { get; }

        public ModelProperty HostView { get; }

        public ModelProperty NodeView { get; }

        public ModelProperty SeedView { get; }

        public UIProperty<CommonAppInfo> AppInfo { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, AppConfig config) 
            : base(lifetimeScope, dispatcher)
        {
            _config = config;
            SetupBuilderView = this.RegisterModel<SetupBuilderViewModel>(nameof(SetupBuilderView), "Setup-Builder");
            NodeView = this.RegisterModel<NodeViewModel>(nameof(NodeView), "Node-View");
            SeedView = this.RegisterModel<SeedNodeViewModel>(nameof(SeedView), "Seed-View");
            HostView = this.RegisterModel<HostViewModel>(nameof(HostView), "Host-View");
            ApplicationsView = this.RegisterModel<ApplicationsViewModel>(nameof(ApplicationsView), "Applications-View");
            ConfigurationView = this.RegisterModel<ConfigurationViewModel>(nameof(ConfigurationView), "Configuration-View");


            AppInfo = this.RegisterImport<CommonAppInfo>(nameof(AppInfo));

            Receive<StartInitialHostSetup>(_ => UICall(() => _dockingManager.ActivateWindow("SetupBuilder")));
            Receive<DisplayApplications>(_ => UICall(() => _dockingManager.ActivateWindow("ApplicationsView")));
        }

        protected override void Initialize(InitEvent evt)
        {
            base.Initialize(evt);

            if (_config.SeedUrls.Count == 0)
            {
                var context = Context;

                Task.Run(() =>
                {
                    Log.Info("No Cluster Found Run Setup");
                    var result = this.ShowDialog<IInitialDialog, string?>()();

                    switch (result)
                    {
                        case "Setup":
                            UICall(() => _dockingManager.ActivateWindow("ConfigurationView"));
                            context.System.EventStream.Publish(StartConfigurationSetup.Get);
                            break;
                    }
                });
            }
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<StartInitialHostSetup>(Self);
            Context.System.EventStream.Unsubscribe<DisplayApplications>(Self);
            base.PostStop();
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<StartInitialHostSetup>(Self);
            Context.System.EventStream.Subscribe<DisplayApplications>(Self);
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
    }
}