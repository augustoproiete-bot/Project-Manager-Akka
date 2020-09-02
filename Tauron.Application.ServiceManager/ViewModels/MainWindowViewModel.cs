using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Syncfusion.Windows.Tools.Controls;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.ServiceManager.Views;
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

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, AppConfig config) 
            : base(lifetimeScope, dispatcher)
        {
            _config = config;
            NodeView = this.RegisterModel<NodeViewModel>(nameof(NodeView), "Node-View");
            SeedView = this.RegisterModel<SeedNodeViewModel>(nameof(SeedView), "Seed-View");
            HostView = this.RegisterModel<HostViewModel>(nameof(HostView), "Host-View");
            ApplicationsView = this.RegisterModel<ApplicationsViewModel>(nameof(ApplicationsView), "Applications-View");
            ConfigurationView = this.RegisterModel<ConfigurationView>(nameof(ConfigurationView), "Configuration-View");

            AppInfo = this.RegisterImport<CommonAppInfo>(nameof(AppInfo));

            Receive<DisplayApplications>(_ =>
            {
                UICall(() => _dockingManager.ActivateWindow("ApplicationsView"));
            });
        }

        protected override void Initialize(InitEvent evt)
        {
            base.Initialize(evt);

            if (_config.SeedUrls.Count == 0)
            {
                Task.Run(() =>
                {
                    var result = this.ShowDialog<IInitialDialog, string?>()();

                    switch (result)
                    {
                        case "Setup":
                            UICall(() => _dockingManager.ActivateWindow("ConfigurationView"));
                            Context.System.EventStream.Publish(new StartConfigurationSetup());
                            break;
                    }
                });
            }
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

        public ModelProperty ConfigurationView { get; }

        public ModelProperty ApplicationsView { get; }

        public ModelProperty HostView { get; }

        public ModelProperty NodeView { get; }

        public ModelProperty SeedView { get; }

        public UIProperty<CommonAppInfo> AppInfo { get; }
    }
}