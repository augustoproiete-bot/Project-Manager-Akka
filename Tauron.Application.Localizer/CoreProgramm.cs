using System.Threading.Tasks;
using Autofac;
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

        public static async Task Main(string[] args)
        {
            var builder = ActorApplication.Create(args);

            builder
               .ConfigureLogging((context, configuration) => configuration.ConfigDefaultLogging("Localizer").WriteTo.Sink<SeriLogViewerSink>())
               .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>().RegisterModule<UIModule>())
               .ConfigurateAkkSystem(((context, system) => system.RegisterLocalization()))
               .UseWpf<MainWindow>();

            using var app = builder.Build();
            await app.Run();
        }
    }
}