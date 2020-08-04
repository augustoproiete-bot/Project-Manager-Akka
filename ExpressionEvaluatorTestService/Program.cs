using System.Threading.Tasks;
using Autofac;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;

namespace ExpressionEvaluatorTestService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Boottrap.StartNode(args, KillRecpientType.Service)
               .ConfigureAutoFac(b =>
                {
                    b.RegisterType<StartExpressionService>().As<IStartUpAction>();
                })
               .Build().Run();
        }
    }
}
