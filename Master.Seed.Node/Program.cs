using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Serilog;

namespace Master.Seed.Node
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

            var config = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("Master.conf"));
            using var master = ActorSystem.Create("Project-Manager", config);

            await master.WhenTerminated;
        }
    }
}
