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
            Console.WriteLine("Downloading Host Data: ");
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
                            Console.WriteLine(Encoding.UTF8.GetString(args.Message.Data));
                            break;
                        case NetworkOperation.Deny:
                            Console.WriteLine("Request Deny");

                            deny = true;

                            if (File.Exists(context.TargetFile))
                                File.Delete(context.TargetFile);

                            context.PhaseLock.Set();
                            break;
                        default:
                            reciver.ProcessMessage(args.Message);
                            break;
                    }
                })) return;
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
                throw;
            }
            finally
            {
                reciver.Dispose();
            }

            if (!deny)
            {
                Console.WriteLine("Download Compled");
                manager.RunNext(context);
            }
            else
                Console.WriteLine("Download Deny");

        }
    }
}