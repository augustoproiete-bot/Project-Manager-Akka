using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Code.Configuration;
using Akka.Code.Configuration.Elements;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Dispatch;
using Akka.Logger.Serilog;
using DotNetty.Transport.Channels.Local;
using Microsoft.Extensions.Configuration;
using Serilog;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.Workflow;
using Tauron.Localization;

namespace AkkaTest
{
    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternalConfigurator : MessageDispatcherConfigurator
    {
        private readonly CallingThreadDispatcherInternal _dispatcher;

        public CallingThreadDispatcherInternalConfigurator(Config config, IDispatcherPrerequisites prerequisites) : base(config, prerequisites)
        {
            _dispatcher = new CallingThreadDispatcherInternal(this);
        }

        public override MessageDispatcher Dispatcher()
        {
            return _dispatcher;
        }
    }

    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternal : MessageDispatcher
    {
        public CallingThreadDispatcherInternal(MessageDispatcherConfigurator configurator)
            : base(configurator)
        {
        }

        protected override void ExecuteTask(IRunnable run)
        {
            run.Run();
        }

        protected override void Shutdown()
        {
        }
    }


    internal class Program
    {
        private class Tester : UntypedActor
        {
            private readonly IActorRef _workflow;

            public Tester() => _workflow = Context.ActorOf(Props.Create<WorkFlowTester>());

            protected override void OnReceive(object message)
            {
                if(message is StartTest st)
                    _workflow.Tell(st);
                else
                    Console.WriteLine(message.ToString());
            }
        }

        private sealed class TestContext : IWorkflowContext
        {
            public string Name { get; set; } = "Unbekannt";

            public StepId ReadName()
            {
                Name = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(Name))
                    return StepId.LoopContinue;
                return Name == "e" ? StepId.Fail : StepId.LoopEnd;
            }
        }

        private sealed class StartTest
        {
            
        }

        private sealed class WorkFlowTester : LambdaWorkflowActor<TestContext>
        {
            private static readonly StepId Terminate = new StepId(nameof(Terminate));
            private static readonly StepId Hallo = new StepId(nameof(Hallo));
            private static readonly StepId Input = new StepId(nameof(Input));

            public WorkFlowTester()
            {
                StartMessage<StartTest>(m => Start(new TestContext()));

                WhenStep(StepId.Start, c => c.OnExecute(_ => Input));

                WhenStep(Input,
                    c =>
                    {
                        c.OnExecute(_ =>
                        {
                            Sender.Tell("Bitte Namen Eingeben:\n");
                            return StepId.Loop;
                        });
                        c.OnNextElement(c => c.ReadName());
                    },
                    con => con.WithCondition().GoesTo(Hallo));

                WhenStep(Hallo,
                    c =>
                        c.OnExecute(ctx =>
                        {
                            Sender.Tell($"Hallo: {ctx.Name}");
                            return Terminate;
                        }));

                WhenStep(Terminate,
                    c =>
                    {
                        c.OnExecute(_ => StepId.Finish);
                        c.OnFinish(_ => Context.System.Terminate());
                    });

                OnFinish(
                    wr =>
                    {
                        if (!wr.Succesfully)
                            Context.System.Terminate();
                    });
            }
        }

        private static async Task Main(string[] args)
        {

            using var testSocked = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            testSocked.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));

            testSocked.Receive(new List<ArraySegment<byte>>(){new ArraySegment<byte>(new byte[10])}, SocketFlags.None, out var error);

            return;
            Console.ReadLine();

            //var test1 = new QueryRegistratedServicesResponse(ImmutableList<MemberService>.Empty
            //   .Add(new MemberService("Test",
            //        MemberAddress.From(
            //            new UniqueAddress(
            //                new Address("akka.tcp", "Project-Manager", "192.178.192.89", 89), 123456)))));



            //ProxyTest.TestProxy();
            //SynchronizationContext.SetSynchronizationContext(new TestSync());
            //AutoFacTest.TestAutofac();

            ////https://github.com/petabridge/akka-bootcamp/blob/master/src/Unit-3/lesson5/README.md
            //var config = ConfigurationFactory.ParseString(File.ReadAllText("akka.config.hocon"));
            var configRoot = new AkkaRootConfiguration();

            //configRoot.Akka.Actor.DefaultDispatcher<DispatcherConfiguration>().Type = typeof(CallingThreadDispatcherInternalConfigurator);

            configRoot.Akka.Loggers.Add(typeof(SerilogLogger));

            var config = configRoot.CreateConfig();

            Log.Logger = new LoggerConfiguration().WriteTo.File("Log.Log").CreateLogger();
            using var system = ActorSystem.Create("Test", config);

            var test = new InternalHostMessages.GetHostNameResult("Test");

            var ser = new InternalSerializer((ExtendedActorSystem)system);

            var test2 = ser.FromBinary<InternalHostMessages.GetHostNameResult>(ser.ToBinary(test));

            //system.ActorOf(Props.Create<Tester>()).Tell(new StartTest());

            Console.WriteLine("Zum Beenden Taste drücken...");
            Console.ReadKey();

            await system.WhenTerminated;
        }

        //[DebuggerNonUserCode]
        //private sealed class TestSync : SynchronizationContext
        //{
        //    public override void Post(SendOrPostCallback d, object? state)
        //    {
        //        d(state);
        //    }

        //    public override void Send(SendOrPostCallback d, object? state)
        //    {
        //        d(state);
        //    }
        //}
    }
}