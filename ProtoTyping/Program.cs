

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Serilog;
using Servicemnager.Networking.Server;
using Servicemnager.Networking.Transmitter;
using Tauron.Application.Master.Commands;

namespace ProtoTyping
{
    internal class Program
    {
        private static BlockingCollection<Action> _dispatcher = new BlockingCollection<Action>();

        private static void ReciverTest(EndPoint adress)
        {
            string testFile = "Test.7z";

            var endppoint = new DataClient(adress.ToString()!.Split(':')[0], int.Parse(adress.ToString()!.Split(':')[1]));
            endppoint.Connect();

            using var waiter = new AutoResetEvent(false);
            using var reciver = new Reciever(() => File.Create(testFile), endppoint);

            endppoint.OnMessageReceived += (sender, args) =>
            {
                _dispatcher.Add(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    if (!reciver.ProcessMessage(args.Message))
                        // ReSharper disable once AccessToDisposedClosure
                        waiter.Set();
                });
            };

            endppoint.Send(NetworkMessage.Create("Start"));

            waiter.WaitOne();
        }

        private static void Sendertest(string file, Action<EndPoint> established)
        {
            var senderWaiter = new AutoResetEvent(false);

            var endpoint = new DataServer("127.0.0.1");
            endpoint.Start();

            Sender testSender = null;
            var pool = ArrayPool<byte>.Shared;

            endpoint.OnMessageReceived += (sender, args) =>
            {
                _dispatcher.Add(() =>
                {
                    switch (args.Message.Type)
                    {
                        case "Start":
                            testSender = new Sender(File.OpenRead(file), args.Client, endpoint, () => pool.Rent(50_000), bytes => pool.Return(bytes), Console.WriteLine);
                            testSender.ProcessMessage(args.Message);
                            break;
                        default:
                            // ReSharper disable once AccessToDisposedClosure
                            if (testSender?.ProcessMessage(args.Message) == false)
                                // ReSharper disable once AccessToDisposedClosure
                                senderWaiter.Set();
                            break;
                    }
                });
            };

            Task.Run(() => established(endpoint.EndPoint));

            senderWaiter.WaitOne();

            endpoint.Dispose();
            testSender?.Dispose();
            senderWaiter.Dispose();
        }

        private static void Main(string[] args)
        {
            Task.Run(() =>
            {
                foreach (var action in _dispatcher.GetConsumingEnumerable())
                {
                    action();
                }
            });

            Sendertest(@"C:\Users\PC\Documents\Backup.7z", ReciverTest);

            return;

            //Log.Logger = new LoggerConfiguration()
            //    .WriteTo.ColoredConsole()
            //    .Enrich.FromLogContext()
            //    .CreateLogger();

            //string url = "akka.tcp://Project-Manager@192.168.105.18:8081";

            //var system = ActorSystem.Create("Project-Manager", ConfigurationFactory.ParseString(await File.ReadAllTextAsync("akka.conf")));

            //var c = Cluster.Get(system);
            ////c.RegisterOnMemberUp(() => ServiceRegistry.Init(system));

            //await c.JoinSeedNodesAsync(new[] { Address.Parse(url) });

            //bool run = true;

            //while (run)
            //{
            //    switch (Console.ReadLine())
            //    {
            //        case "e":
            //            run = false;
            //            break;
            //        case "t":
            //            try
            //            {
            //                var reg = ServiceRegistry.GetRegistry(system);
            //                reg.RegisterService(new RegisterService("Test Service", Cluster.Get(system).SelfUniqueAddress));

            //                var r = await reg.QueryService();

            //                Console.WriteLine();
            //                foreach (var mem in r.Services)
            //                    Console.WriteLine($"{mem.Name} -- {mem.Address}");

            //                Console.WriteLine();
            //                Console.WriteLine("Fertig");
            //            }
            //            catch(Exception e)
            //            {
            //                Console.WriteLine(e.ToString());
            //                run = false;
            //            }
            //            break;
            //        default:
            //            Console.WriteLine("Unbekannt");
            //            break;

            //    }
            //}

            //await system.Terminate();
        }
    }
}