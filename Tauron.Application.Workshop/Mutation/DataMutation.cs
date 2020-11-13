using System;
using System.Threading.Tasks;
using Akka.Routing;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IDataMutation : IConsistentHashable
    {
        string Name { get; }

    }

    public interface ISyncMutation : IDataMutation
    {
        Action Run { get; }
    }

    public interface IAsyncMutation : IDataMutation
    {
        Func<Task> Run { get; }
    }

    [PublicAPI]
    public sealed record DataMutation(Action Task, string Name, Maybe<object> Hash = default) : ISyncMutation
    {
        public object ConsistentHashKey => Hash.Or(Name);

        Action ISyncMutation.Run => Task;
    }
    
    [PublicAPI]
    public sealed record AsyncDataMutation(Func<Task> Task, string Name, Maybe<object> Hash = default) : IAsyncMutation
    {
        public object ConsistentHashKey => Hash.Or(Name);
        Func<Task> IAsyncMutation.Run => Task;
    }
}