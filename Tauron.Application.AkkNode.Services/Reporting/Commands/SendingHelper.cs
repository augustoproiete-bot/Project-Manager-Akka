using System;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Akka;
using Tauron.Host;

namespace Tauron.Application.AkkNode.Services.Commands
{
    public static class SendingHelper
    {
        public static Task<TResult> Send<TResult, TCommand>(ISender sender, TCommand command, Action<string> messages, TimeSpan timeout, bool isEmpty) 
            where TCommand : class, IReporterMessage
        {
            var task = new TaskCompletionSource<TResult>();

            IActorRefFactory factory;

            try
            {
                factory = ExposedReceiveActor.ExposedContext;
            }
            catch (NotSupportedException)
            {
                factory = ActorApplication.Application.ActorSystem;
            }

            var listner = Reporter.CreateListner(factory, messages, result =>
            {
                if (result.Ok)
                {
                    if(isEmpty)
                        task.SetResult(default!);
                    else if (result.Outcome is TResult outcome)
                    {
                        task.SetResult(outcome);
                    }
                    else
                        task.SetException(new InvalidCastException(result.Outcome?.GetType().Name ?? "null-source"));
                }
                else
                {
                    task.SetException(new CommandFailedException(result.Error ?? "Unkowen"));
                }
            }, timeout, null);
            command.SetListner(listner);

            sender.SendCommand(command);

            return task.Task;
        }
    }
}