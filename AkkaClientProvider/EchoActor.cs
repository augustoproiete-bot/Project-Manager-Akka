using System;
using Akka.Actor;
using AkkaShared;

namespace AkkaClientProvider
{
    public sealed class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<StringMessage>(Handle);
        }

        private void Handle(StringMessage obj)
        {
            Console.WriteLine(obj.Message);
            Context.Sender.Tell(new StringMessage("Hello from Service"));
        }
    }
}