using System;
using ServiceManagerIpProbe.Phases;
using Servicemnager.Networking;
using Servicemnager.Networking.Server;

namespace ServiceManagerIpProbe.Phase
{
    public class SetConfigAndConnectPhase : Phase<OperationContext>
    {
        public override void Run(OperationContext context, PhaseManager<OperationContext> manager)
        {
            context.WriteLine("Read Configuration");

            context.Configuration = HostConfiguration.Read();

            var data = context.Configuration.TargetAdress.Split(':');

            var port = int.Parse(data[1]);

            context.Write("Open Server ");

            var client = new DataClient(data[0], port);
            client.Connect();

            context.DataClient = client;

            manager.RunNext(context);
        }
    }
}