using System;
using System.IO;

namespace ServiceHost.Installer.Impl.Source
{
    public static class InstallationSourceSelector
    {
        public static IInstallationSource Select(InstallerContext context)
        {
            if(context.SourceLocation is string)
                return new LocalSource();

            return EmptySource.Instnace;
        }
    }
}