using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Cluster;
using Akka.Configuration;

namespace ProtoTyping
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var sys = ActorSystem.Create("ClusterSystem", ConfigurationFactory.ParseString(await File.ReadAllTextAsync("test.conf")));
            
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await Cluster.Get(sys).JoinAsync(Address.Parse("akka.tcp://ClusterSystem@localhost:8081"), source.Token)
               .ContinueWith(t =>
                             {
                                 var temp = t.Status;
                             });

            Console.ReadKey();
            await sys.Terminate();
        }
    }
}