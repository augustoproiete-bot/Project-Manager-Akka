using System;
using System.IO;
using System.Text;
using ServiceManagerIpProbe.Phases;
using Servicemnager.Networking;
using Servicemnager.Networking.Server;
using Servicemnager.Networking.Transmitter;

namespace ServiceManagerIpProbe.Phase
{
    public sealed class TryGetDataPhase : Phase<OperationContext>
    {
        public override void Run(OperationContext context, PhaseManager<OperationContext> manager)
        {
            context.WriteLine("Downloading Host Data: ");
            var deny = false;

            context.TargetFile = Path.GetTempFileName();

            var id = NetworkMessage.Create(NetworkOperation.Identifer, Encoding.UTF8.GetBytes(context.Configuration.Identifer));
            var reciver = new Reciever(() => File.Create(context.TargetFile), context.DataClient);

            try
            {
                if (!context.ProcessAndWait(id, (sender, args) =>
                {
                    switch (args.Message.Type)
                    {
                        case NetworkOperation.Message:
                            context.WriteLine(Encoding.UTF8.GetString(args.Message.Data));
                            break;
                        case NetworkOperation.Deny:
                            context.WriteLine("Request Deny");

                            deny = true;

                            if (File.Exists(context.TargetFile))
                                File.Delete(context.TargetFile);

                            context.PhaseLock.Set();
                            break;
                        default:
                            if (!reciver.ProcessMessage(args.Message))
                                context.PhaseLock.Set();
                            break;
                    }
                })) return;
            }
            catch (Exception)
            {
                context.WriteLine("Error");
                throw;
            }
            finally
            {
                reciver.Dispose();
            }

            if (!deny)
            {
                context.WriteLine("Download Compled");
                manager.RunNext(context);
            }
            else
                context.WriteLine("Download Deny");

        }
    }
}