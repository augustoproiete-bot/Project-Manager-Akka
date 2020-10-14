using System;
using System.Threading.Tasks;
using Akka.Actor.Dsl;
using Akka.Event;
using Tauron.Akka;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core
{
    public static class CommandQueryBuilderExtensions
    {
        public static CommandQuery FromEvent<TEvent>(this CommandQueryBuilder builder, Task<bool> currentValue, Func<TEvent, bool> check)
        {
            var context = ExposedReceiveActor.ExposedContext;

            context.ActorOf(dsl =>
            {
                dsl.OnPreStart += actorContext => actorContext.System.EventStream.Subscribe<TEvent>(actorContext.Self);
                dsl.Receive<TEvent>();
            });
        }
    }
}