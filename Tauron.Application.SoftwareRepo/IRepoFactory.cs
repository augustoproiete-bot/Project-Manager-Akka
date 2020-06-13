using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.SoftwareRepo
{
    [PublicAPI]
    public interface IRepoFactory
    {
        SoftwareRepository Create(IActorRefFactory factory, IDirectory path);
        SoftwareRepository Read(IActorRefFactory factory, IDirectory path);
        bool IsValid(IDirectory path);

        SoftwareRepository Create(IActorRefFactory factory, string path);

        SoftwareRepository Read(IActorRefFactory factory, string path);
        bool IsValid(string path);
    }
}