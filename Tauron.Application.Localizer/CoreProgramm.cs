using System;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Syncfusion.Licensing;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Logging;
using Tauron.Application.Wpf.SerilogViewer;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Localizer
{
    public static class CoreProgramm
    {
        //static string MakeRelativePath(string absolutePath, string pivotFolder)
        //{
        //    //string folder = Path.IsPathRooted(pivotFolder)
        //    //    ? pivotFolder : Path.GetFullPath(pivotFolder);
        //    string folder = pivotFolder;
        //    Uri pathUri = new Uri(absolutePath);
        //    // Folders must end in a slash
        //    if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        //    {
        //        folder += Path.DirectorySeparatorChar;
        //    }
        //    Uri folderUri = new Uri(folder);
        //    Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
        //    return Uri.UnescapeDataString(
        //        relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        //}
        //"pack: //application:,,,/Tauron.Application.Localizer;component/Theme.xaml"

        public static async Task Main(string[] args)
        {
            SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            var builder = ActorApplication.Create(args);

            builder
                .ConfigureLogging((context, configuration) => configuration.ConfigDefaultLogging("Localizer").WriteTo.Sink<SeriLogViewerSink>())
                .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>().RegisterModule<UIModule>())
                .ConfigurateAkkSystem((context, system) => system.RegisterLocalization())
                .UseWpf<MainWindow>(c => c.WithAppFactory(() =>
                {
                    var runapp = new System.Windows.Application
                    {
                        Resources = new ResourceDictionary
                        {
                            Source = new Uri("/Theme.xaml", UriKind.Relative)
                        }
                    };


                    return runapp;
                }));

            using var app = builder.Build();
            await app.Run();
        }
    }
}