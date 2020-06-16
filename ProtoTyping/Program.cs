using System;
using System.IO;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Cluster;
using Akka.Configuration;

namespace ProtoTyping
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sys = ActorSystem.Create("ClusterSystem", ConfigurationFactory.ParseString(File.ReadAllText("test.conf")));

            var test = Cluster.Get(sys).JoinAsync(Address.Parse("akka.tcp://ClusterSystem@localhost:8081"))
               .ContinueWith(t =>
                             {
                                 var temp = t.Status;
                             });

            Console.ReadKey();
            sys.Terminate().Wait();
        }
    }
}