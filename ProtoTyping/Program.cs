using System;
using Akka.Actor;
using Tauron;
using Tauron.Akka;

namespace ProtoTyping
{
    public sealed class TestMsg
    {
        public string Message { get; }

        public TestMsg(string message) => Message = message;
    }

    public sealed class TestActor : ExposedReceiveActor
    {
        public TestActor()
        {
            this.Flow<TestMsg>()
               .To.Func(tm => tm.Message).ToSelf()
               .Then.Action(TestMsg).Receive();
        }

        private void TestMsg(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg + " From Actor");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using var system = ActorSystem.Create("Test");

            var test = system.ActorOf<TestActor>();

            test.Tell(new TestMsg(Console.ReadLine()));
            Console.ReadKey();
        }
    }
}
