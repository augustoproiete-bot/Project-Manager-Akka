using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Akka.Actor;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class LocalSource : IInstallationSource
    {
        public Status ValidateInput(InstallerContext context)
        {
            try
            {
                ZipFile.OpenRead((string)context.SourceLocation).Dispose();
                return new Status.Success(null);
            }
            catch (Exception e)
            {
                return new Status.Failure(e);
            }
        }

        public Task<Status> PreperforCopy(InstallerContext context) 
            => Task.FromResult<Status>(new Status.Success(null));

        public Task<Status> CopyTo(InstallerContext context, string target)
        {
            return Task.Run<Status>(() =>
            {
                try
                {
                    ZipFile.ExtractToDirectory((string)context.SourceLocation, target, context.Override);
                    return new Status.Success(null);
                }
                catch (Exception e)
                {
                    return new Status.Failure(e);
                }
            });
        }

        public void CleanUp(InstallerContext context)
        {

        }

        public int Version { get; } = 0;
        public string ToZipFile(InstallerContext context) => (string)context.SourceLocation;
    }
}