using JetBrains.Annotations;
using Tauron;
using Tauron.Akka;

namespace ServiceHost.AutoUpdate
{
    [UsedImplicitly]
    public sealed class AutoUpdateActor : ExposedReceiveActor
    {
        public AutoUpdateActor()
        {
            this.Flow<>()
        }
    }
}