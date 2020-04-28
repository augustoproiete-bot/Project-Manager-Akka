using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Code.Configuration;
using Akka.Code.Configuration.Elements;
using Akka.Code.Configuration.Extensions;

namespace Akka.Copy.Server
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new AkkaRootConfiguration();
            config.Akka.StdoutLoglevel = AkkaLogLevel.Info;
            config.AddHyperion();

            using var system = ActorSystem.Create("tauron-copy-server", config.CreateConfig());

            Console.WriteLine("Press \'c\' to close App");
            while (Console.ReadKey().KeyChar != 'c')
            { }

            await system.Terminate();
        }
    }
}
