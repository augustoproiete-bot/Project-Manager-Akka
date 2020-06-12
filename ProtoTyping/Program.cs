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

            Console.ReadKey();
            sys.Terminate().Wait();
        }
    }
}