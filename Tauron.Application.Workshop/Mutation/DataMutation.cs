using System;
using Akka.Routing;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class DataMutation<TData> : IConsistentHashable
    {
        private readonly object? _hash;

        public DataMutation(Func<TData, TData> mutatuion, Func<TData> receiver, Action<TData> responder, string name, object? hash = null)
        {
            _hash = hash;
            Mutatuion = mutatuion;
            Receiver = receiver;
            Responder = responder;
            Name = name;
        }

        public string Name { get; }

        public Func<TData, TData> Mutatuion { get; }

        public Func<TData> Receiver { get; }

        public Action<TData> Responder { get; }
        public object ConsistentHashKey => _hash ?? Name;
    }
}