using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.Cluster;
using Autofac;
using Syncfusion.Licensing;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands;
using Tauron.Application.ServiceManager.PreAppStart;
using Tauron.Application.Wpf.SerilogViewer;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.ServiceManager
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            switch (IpConfigurationChecker.CheckConfiguration())
            {
                case null:
                    Environment.ExitCode = -1;
                    return;
                case true:
                    await ActorApplication.Create(args)
                        .ConfigurateNode()
                        .ConfigureLogging((context, configuration) => configuration.WriteTo.Sink<SeriLogViewerSink>())
                        .ConfigurateAkkaSystem((context, system) => system.RegisterLocalization())
                        .ConfigurateAkkaSystem((context, system) => KillSwitch.Subscribe(system, KillRecpientType.Frontend))
                        .ConfigurateAkkaSystem(
                            (context, system) =>
                                Cluster.Get(system)
                                    .RegisterOnMemberUp(
                                        () => ServiceRegistry.GetRegistry(system).RegisterService(new RegisterService(context.HostEnvironment.ApplicationName, Cluster.Get(system).SelfUniqueAddress))))
                        .ConfigureAutoFac(cb => cb.RegisterModule<CoreModule>())
                        .UseWpf<MainWindow>(configuration => configuration.WithAppFactory(() => new App()))
                        .Build().Run();
                    break;
                case false:
                    Process.Start(System.Windows.Forms.Application.ExecutablePath.Replace(".dll", ".exe"), Environment.CommandLine);
                    break;
            }
        }
    }
}