using System;
using System.IO;
using System.Text;
using ServiceManagerIpProbe.Phases;
using Servicemnager.Networking.Server;

namespace ServiceManagerIpProbe.Phase
{
    public sealed class TryGetDataPhase : Phase<OperationContext>
    {
        public override void Run(OperationContext context, PhaseManager<OperationContext> manager)
        {
            Console.Write("Downloading Host Data: ");
            var deny = false;

            context.TargetFile = Path.GetTempFileName();

            var id = NetworkMessage.Create(Operation.Identifer, Encoding.UTF8.GetBytes(context.Configuration.Identifer));
            Stream dataStream = null;


            try
            {
                if (!context.ProcessAndWait(id, (sender, args) =>
                {
                    switch (args.Message.Type)
                    {
                        case Operation.Deny:
                            deny = true;
                            context.PhaseLock.Set();
                            break;
                        case Operation.Compled:
                            context.PhaseLock.Set();
                            break;
                        case Operation.Accept:
                            dataStream = File.Open(context.TargetFile, FileMode.Open);
                            break;
                        case Operation.Data:
                            // ReSharper disable once AccessToDisposedClosure
                            dataStream?.Write(args.Message.Data, 0, args.Message.Data.Length);
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
                dataStream?.Dispose();
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