using System;
using Akka.Routing;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IDataMutation : IConsistentHashable
    {
        string Name { get; }

        Action Run { get; }
    }

    public sealed class DataMutation<TData> : IDataMutation
        where TData : class
    {
        private readonly object? _hash;

        public DataMutation(Func<TData, TData?> mutatuion, Func<TData> receiver, Action<TData?> responder, string name, object? hash = null)
        {
            _hash = hash;
            Mutatuion = mutatuion;
            Receiver = receiver;
            Responder = responder;
            Name = name;
        }

        public string Name { get; }
        Action IDataMutation.Run => () => Responder(Mutatuion(Receiver()));

        public Func<TData, TData> Mutatuion { get; }

        public Func<TData> Receiver { get; }

        public Action<TData> Responder { get; }
        public object ConsistentHashKey => _hash ?? Name;
    }
}