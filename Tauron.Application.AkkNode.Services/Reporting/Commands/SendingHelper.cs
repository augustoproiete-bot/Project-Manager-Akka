using System;
using System.Threading.Tasks;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.Commands
{
    public static class SendingHelper
    {
        public static Task<TResult> Send<TResult, TCommand>(ISender sender, TCommand command, Action<string> messages, TimeSpan timeout, bool isEmpty) 
            where TCommand : class, IReporterMessage
        {
            var task = new TaskCompletionSource<TResult>();

            var listner = Reporter.CreateListner(ExposedReceiveActor.ExposedContext, messages, result =>
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
            }, timeout);
            command.SetListner(listner);

            sender.SendCommand(command);

            return task.Task;
        }
    }
}