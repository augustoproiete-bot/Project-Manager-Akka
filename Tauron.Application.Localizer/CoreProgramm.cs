using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Tauron.Application.Logging;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Localizer
{
    public static class CoreProgramm
    {
        static string MakeRelativePath(string absolutePath, string pivotFolder)
        {
            //string folder = Path.IsPathRooted(pivotFolder)
            //    ? pivotFolder : Path.GetFullPath(pivotFolder);
            string folder = pivotFolder;
            Uri pathUri = new Uri(absolutePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(
                relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static async Task Main(string[] args)
        {
            var builder = ActorApplication.Create(args);

            builder
               .ConfigureLogging(((context, configuration) => configuration.ConfigDefaultLogging("Localizer")))
               .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>())
               .ConfigurateAkkSystem(((context, system) => system.RegisterLocalization()))
               .UseWpf<MainWindow>(c => c.WithAppFactory(() => new App()));

            using var app = builder.Build();
            await app.Run();
        }
    }
}