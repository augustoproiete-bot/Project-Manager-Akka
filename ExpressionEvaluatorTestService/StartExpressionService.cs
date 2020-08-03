using Akka.Actor;
using Akka.Cluster;
using ExpressionEvaluatorTestService.Api;
using Tauron.Application.AkkaNode.Boottrap;

namespace ExpressionEvaluatorTestService
{
    public sealed class StartExpressionService : IStartUpAction
    {
        private readonly ActorSystem _system;

        public StartExpressionService(ActorSystem system)
        {
            _system = system;
        }

        public void Run()
        {
            Cluster.Get(_system).RegisterOnMemberUp(() => ExpressionApi.Start(_system, Props.Create<ActualEvaluator>()));
        }
    }
}