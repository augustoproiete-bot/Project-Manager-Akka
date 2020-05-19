using System;
using Akka.Actor;
using Akka.Event;

namespace Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine
{
    public sealed class MutationActor<TData> : ReceiveActor
    {
        private sealed class HandlerTerminated
        {
            public Action Remover { get; }

            public HandlerTerminated(Action remover) => Remover = remover;
        }

        private ILoggingAdapter _log => Context.GetLogger();

        public MutationActor()
        {
            Receive<DataMutation<TData>>(Mutation);
            Receive<WatchIntrest>(wi => Context.WatchWith(wi.Target, new HandlerTerminated(wi.OnRemove)));
            Receive<HandlerTerminated>(ht => ht.Remover());
            Receive<Terminated>(t =>{});

        }

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
    }
}