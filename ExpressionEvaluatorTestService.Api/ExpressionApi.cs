using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;
using JetBrains.Annotations;

namespace ExpressionEvaluatorTestService.Api
{
    [PublicAPI]
    public sealed class ExpressionApi
    {
        public static void Start(ActorSystem system, Props actor)
            => ClusterSharding.Get(system).Start(nameof(ExpressionApi), actor, ClusterShardingSettings.Create(system), new ExpressionMessageExtractor());

        public static ExpressionApi Connect(ActorSystem system)
            => new ExpressionApi(ClusterSharding.Get(system).StartProxy(nameof(ExpressionApi), null, new ExpressionMessageExtractor()));

        private readonly IActorRef _actor;

        private ExpressionApi(IActorRef actor)
        {
            _actor = actor;
        }

        public void Evaluate(string data, bool script) 
            => _actor.Tell(new EvaluateExpression(data, script));

        public Task<EvaluationResult> AskEvaluate(string data, bool script) 
            => _actor.Ask<EvaluationResult>(new EvaluateExpression(data, script), TimeSpan.FromMinutes(2));
    }
}