using System;
using System.Threading.Tasks;
using Akka.Routing;

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

    public sealed class DataMutation<TData> : ISyncMutation
        where TData : class
    {
        private readonly Action _task;
        private readonly object? _hash;

        public DataMutation(Action task, string name, object? hash = null)
        {
            _task = task;
            _hash = hash;
            Name = name;
        }

        public string Name { get; }
        Action ISyncMutation.Run => _task;

        public object ConsistentHashKey => _hash ?? Name;
    }

    public sealed class AsyncDataMutation<TData> : IAsyncMutation
        where TData : class
    {
        private readonly Func<Task> _task;
        private readonly object? _hash;

        public AsyncDataMutation(Func<Task> task, string name, object? hash = null)
        {
            _task = task;
            _hash = hash;
            Name = name;
        }

        public string Name { get; }

        public object ConsistentHashKey => _hash ?? Name;
        Func<Task> IAsyncMutation.Run => _task;
    }
}