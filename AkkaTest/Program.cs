using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Akka.Actor;
using Tauron.Application.Akka.ServiceResolver;

namespace AkkaTest
{
    public sealed class TestActor : ReceiveActor
    {
        public TestActor()
        {
            Receive<string>(Console.WriteLine);
            Receive<Test>(m => Context.Self.Tell(m.Msg));
        }
    }

    public sealed class ErrorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class TestCommander : ReceiveActor
    {
        public TestCommander()
        {
            Receive<Terminated>(TerminatedHandler);
            ReceiveAny(AnyHandler);
        }

        private void AnyHandler(object obj)
        {
            var op = Context.ActorOf<ErrorActor>(Guid.NewGuid().ToString());
            Context.Watch(op);
            op.Tell(obj);
        }

        private void TerminatedHandler(Terminated obj)
        {
            var test = obj.ActorRef.Path.Name;
        }

        protected override SupervisorStrategy SupervisorStrategy() 
            => new OneForOneStrategy(exception => Directive.Stop);
    }

    class Program
    {
        static void Main(string[] args)
        {
            ////https://github.com/petabridge/akka-bootcamp/blob/master/src/Unit-3/lesson5/README.md
            ////var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.config.hocon"));
            //var configRoot = new AkkaRootConfiguration();
            //var mailbox = new BoundedMailbox(100, TimeSpan.FromSeconds(5));
            //configRoot.Add("test-mailbox", mailbox);

            //var stream = new MemoryStream();
            //var ser = new ConfigSerializer();

            //ser.Write(stream, configRoot);
            //stream = new MemoryStream(stream.ToArray());
            //configRoot = ser.Read(stream);

            //var config = configRoot.CreateConfig();

            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("127.0.0.1", 8090);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }

            using var system = ActorSystem.Create("Test");
            var exz = system.AddServiceResolver();

            system.ActorOf<TestCommander>().Tell("Hallo");

            var prop = Props.Create<TestActor>();
            var actor = system.ActorOf(prop, "TestActor");

            actor.Tell(new Test("Hallo Welt"));
            actor.Tell(actor.Path.ToString());

            Console.ReadKey();
            system.Terminate();
        }
    }
}
