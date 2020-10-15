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
        public static CommandQuery FromEvent<TEvent>(this CommandQueryBuilder builder, Task<TEvent> currentValue, Func<TEvent?, bool> check)
            where TEvent : class
        {
            var context = ExposedReceiveActor.ExposedContext;

            return builder.FromExternal(check, action =>
            {
                currentValue.ContinueWith(t =>
                {
                    if(t.IsCompleted)
                        action(t.Result);
                });

                context.ActorOf(dsl =>
                {
                    dsl.OnPreStart += actorContext => actorContext.System.EventStream.Subscribe<TEvent>(actorContext.Self);
                    dsl.OnPostStop += actorContext => actorContext.System.EventStream.Unsubscribe(actorContext.Self);
                    dsl.Receive<TEvent>((e, c) => action(e));
                });
            });
        }
    }
}