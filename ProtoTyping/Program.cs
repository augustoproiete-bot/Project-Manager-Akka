
using System.Linq;
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
using MongoDB.Driver;
using Octokit;
using Serilog;
using ServiceManager.ProjectRepository.Data;
using Tauron;
using Tauron.Application.Master.Commands;

namespace ProtoTyping
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("Test-App"));

            var test = gitHubClient.Repository.Commit.Get("octokit", "octokit.net", "HEAD").Result;

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