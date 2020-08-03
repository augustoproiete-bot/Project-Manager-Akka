using Akka.Cluster.Sharding;

namespace ExpressionEvaluatorTestService.Api
{
    public sealed class ExpressionMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message) => "1";

        public object EntityMessage(object message) => message;

        public string ShardId(object message) => "1";
    }
}