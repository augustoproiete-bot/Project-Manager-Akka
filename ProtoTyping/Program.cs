using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Serilog;
using ServiceManager.ProjectRepository;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Repository;

namespace ProtoTyping
{
    internal class Program
    {
        private sealed class TestActor : ReceiveActor
        {
            private const string TargetRepo = "Tauron1990/Project-Manager-Akka";
            private const string TargetFile = "Test.zip";

            private readonly IActorRef _dataTransfer;

            private RepositoryManager _operator = RepositoryManager.Empty;
            private bool _cancel;
            private ManualResetEvent? _currentTransfer;

            public TestActor()
            {
                _dataTransfer = DataTransferManager.New(Context);
                _dataTransfer.SubscribeToEvent<TransferCompled>();
                _dataTransfer.SubscribeToEvent<TransferFailed>();
                _dataTransfer.SubscribeToEvent<IncomingDataTransfer>();

                Receive<IncomingDataTransfer>(d => d.Accept(() => File.Create(TargetFile)));

                Receive<TransferMessages.TransferCompled>(c =>
                {
                    if(_currentTransfer == null)
                        return;

                    try
                    {
                        switch (c)
                        {
                            case TransferCompled _:
                                ZipFile.Open(TargetFile, ZipArchiveMode.Read).Dispose();
                                Console.WriteLine("Tranfer Beended");
                                break;
                            case TransferFailed failed:
                                Console.WriteLine($"Transfer Fehler: {failed.Reason}");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        Self.Tell(_currentTransfer);
                    }
                });



                var toDo = new Queue<Action<ManualResetEvent>>();
                toDo.Enqueue(Init);
                toDo.Enqueue(RegisterTest);
                toDo.Enqueue(RegisterTest);
                toDo.Enqueue(TransferTest);
                toDo.Enqueue(TransferTest);
                toDo.Enqueue(CleanTest);

                Receive<ManualResetEvent>(evt =>
                {
                    if (_cancel || toDo.Count == 0)
                    {
                        evt.Set();
                        return;
                    }

                    var op = toDo.Dequeue();
                    op(evt);
                    Thread.Sleep(2000);
                });
            }

            private void Init(ManualResetEvent evt)
            {
                try
                {
                    const string work = "192.168.105.96";
                    _operator = RepositoryManager.CreateInstance(Context, "mongodb://127.0.0.1:27017/?readPreference=primary&ssl=false");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    _cancel = true;
                }
                finally
                {
                    Self.Tell(evt);
                }
            }

            private void RegisterTest(ManualResetEvent evt)
            {
                try
                {
                    var awaiter = new ManualResetEventSlim();
                    var reporter = Reporter.CreateListner(Context, Console.WriteLine, result =>
                    {
                        Console.WriteLine(result.Ok ? "Fertig" : $"Fehler {result.Error}");
                        awaiter.Set();
                    });

                    _operator.SendAction(new RegisterRepository(TargetRepo, reporter));

                    awaiter.Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    _cancel = true;
                }
                finally
                {
                    Self.Tell(evt);
                }
            }

            private void TransferTest(ManualResetEvent evt)
            {
                try
                {
                    var reporter = Reporter.CreateListner(Context, Console.WriteLine, result => Console.WriteLine(result.Ok ? "Fertig" : $"Fehler {result.Error}"));

                    _operator.SendAction(new TransferRepository(TargetRepo, reporter, _dataTransfer));

                    _currentTransfer = evt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    _cancel = true;
                }
            }

            private void CleanTest(ManualResetEvent evt)
            {
                try
                {
                    _operator.CleanUp();
                    Thread.Sleep(TimeSpan.FromMinutes(2));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    _cancel = true;
                }
                finally
                {
                    Self.Tell(evt);
                }
            }
        }

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().CreateLogger();
            using var evt = new ManualResetEvent(false);

            var system = ActorSystem.Create("Test", ConfigurationFactory.ParseString(File.ReadAllText("akka.conf")));
            system.ActorOf(Props.Create<TestActor>()).Tell(evt);

            evt.WaitOne();
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