using Autofac;
using JetBrains.Annotations;

namespace Tauron.Application.SoftwareRepo
{
    [PublicAPI]
    public sealed class SoftwareRepoModule : Module
    {
        protected override void Load(ContainerBuilder builder) 
            => builder.RegisterType<RepoFactory>().As<IRepoFactory>();
    }
}