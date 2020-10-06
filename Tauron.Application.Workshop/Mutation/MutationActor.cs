using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class MutationActor<TData> : ReceiveActor
    {
        private ImmutableDictionary<IActorRef, Action> _intrest = ImmutableDictionary<IActorRef, Action>.Empty;

        public MutationActor()
        {
            Receive<DataMutation<TData>>(Mutation);
            Receive<WatchIntrest>(wi =>
            {
                ImmutableInterlocked.AddOrUpdate(ref _intrest, wi.Target, _ => wi.OnRemove, (_, action) => action.Combine(wi.OnRemove) ?? wi.OnRemove);
                Context.Watch(wi.Target);
            });
            Receive<Terminated>(t =>
            {
                if (!_intrest.TryGetValue(t.ActorRef, out var action)) return;
                
                action();
                _intrest = _intrest.Remove(t.ActorRef);
            });
        }

        private ILoggingAdapter _log => Context.GetLogger();

        private void Mutation(DataMutation<TData> obj)
        {
            try
            {
                _log.Info("Mutation Begin: {Name}", obj.Name);
                obj.Responder(obj.Mutatuion(obj.Receiver()));
                _log.Info("Mutation Finisht: {Name}", obj.Name);
            }
            catch (Exception e)
            {
                _log.Error(e, "Mutation Failed: {Name}", obj.Name);
            }
        }

        //private sealed class HandlerTerminated
        //{
        //    public HandlerTerminated(Action remover)
        //    {
        //        Remover = remover;
        //    }

        //    public Action Remover { get; }
        //}
    }
}