using System;
using Akka.Actor;
using Akka.Util.Internal;
using Tauron.Application.Akka.ServiceResolver.Messages;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public sealed class ServiceHostActor : ReceiveActor
    {
        private readonly Props _serviceType;

        public ServiceHostActor(Props serviceType)
        {
            _serviceType = serviceType;

            Become(Running);
        }

        private void Running()
        {
            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            ReceiveAny(m => GetService().Forward(m));

        }

        private void Suspended()
        {
            Receive<ToggleSuspendedMessage>(ToggleSuspendedMessage);
            ReceiveAny(m => Context.Sender.Tell(new ServiceCallRejected(m, ServiceCallRejected.Suspended)));
        }

        private void ToggleSuspendedMessage(ToggleSuspendedMessage obj)
        {
            if(obj.IsSuspended)
                Become(Suspended);
            else
                Become(Running);
        }

        private IActorRef GetService()
        {
            const string name = "ServiceRoot";
            var service = Context.Child(name);
            if (service.Equals(ActorRefs.Nobody))
                service = Context.ActorOf(_serviceType);
            return service;
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Context.GetChildren().ForEach(c => c.Tell(PoisonPill.Instance));
            Context.Sender.Tell(new ServiceCallRejected(message, ServiceCallRejected.Error, reason));
        }
    }
}