using System.Threading.Tasks;
using Autofac;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands;

namespace ExpressionEvaluatorTestService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Bootstrap.StartNode(args, KillRecpientType.Service)
               .ConfigureAutoFac(b =>
                {
                    b.RegisterType<StartExpressionService>().As<IStartUpAction>();
                })
               .Build().Run();
        }
    }
}
