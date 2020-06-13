using Akka.Actor;
using Tauron.Application.Files.VirtualFiles;

namespace Tauron.Application.SoftwareRepo
{
    public sealed class RepoFactory : IRepoFactory
    {
        public SoftwareRepository Create(IActorRefFactory factory, IDirectory path)
        {
            var temp = new SoftwareRepository(factory, path);
            temp.InitNew();
            return temp;
        }

        public SoftwareRepository Read(IActorRefFactory factory, IDirectory path)
        {
            var temp = new SoftwareRepository(factory, path);
            temp.Init();
            return temp;
        }

        public bool IsValid(IDirectory path) 
            => path.GetFile(SoftwareRepository.FileName).Exist;

        public SoftwareRepository Create(IActorRefFactory factory, string path) 
            => Create(factory, VirtualFileFactory.CrerateLocal(path));

        public SoftwareRepository Read(IActorRefFactory factory, string path) 
            => Read(factory, VirtualFileFactory.CrerateLocal(path));

        public bool IsValid(string path)
            => IsValid(VirtualFileFactory.CrerateLocal(path));
    }  
}