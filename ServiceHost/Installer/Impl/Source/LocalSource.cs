using System;
using System.IO.Compression;
using Akka.Actor;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class LocalSource : IInstallationSource
    {
        public Status ValidateInput(InstallerContext context)
        {
            try
            {
                ZipFile.OpenRead(context.Path).Dispose();
                return new Status.Success(null);
            }
            catch (Exception e)
            {
                return new Status.Failure(e);
            }
        }
    }
}