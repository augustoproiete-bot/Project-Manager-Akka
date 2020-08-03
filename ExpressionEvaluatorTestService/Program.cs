using System;
using System.Threading.Tasks;
using Autofac;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace ExpressionEvaluatorTestService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
               .ConfigureAutoFac(b =>
                {
                    b.RegisterType<StartExpressionService>().As<IStartUpAction>();
                })
               .StartNode(KillRecpientType.Service)
               .Build().Run();
        }
    }
}
