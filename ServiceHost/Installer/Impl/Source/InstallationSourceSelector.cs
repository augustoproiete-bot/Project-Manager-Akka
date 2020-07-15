using System.IO;

namespace ServiceHost.Installer.Impl.Source
{
    public static class InstallationSourceSelector
    {
        public static IInstallationSource Select(InstallerContext context)
        {
            if(File.Exists(context.Path))
                return new LocalSource();

            return EmptySource.Instnace;
        }
    }
}