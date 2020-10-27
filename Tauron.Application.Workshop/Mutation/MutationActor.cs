using System;
using Akka.Actor;
using Akka.Event;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class MutationActor : ReceiveActor
    {
        public MutationActor() => Receive<IDataMutation>(Mutation);

        private ILoggingAdapter _log => Context.GetLogger();

        private void Mutation(IDataMutation obj)
        {
            try
            {
                _log.Info("Mutation Begin: {Name}", obj.Name);
                obj.Run();
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