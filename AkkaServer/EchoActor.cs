using System;
using Akka.Actor;

namespace AkkaShared
{
    /// <summary>
    /// Actor that just replies the message that it received earlier
    /// </summary>
    public class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<Hello>(hello =>
            {
                Console.WriteLine("[{0}]: {1}", Sender, hello.Message);
                Sender.Tell(hello);
            });
        }
    }
}
