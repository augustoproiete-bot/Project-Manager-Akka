using System;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services
{
    public abstract class ReportingActor : ExposedReceiveActor
    {
        protected void Receive<TMessage>(string name, Action<TMessage, Reporter> process) 
            where TMessage : IReporterMessage
        {
            Receive<TMessage>(m => TryExecute(m, name, process));
        }
        protected void ReceiveContinue<TMessage>(string name, Action<TMessage, Reporter> process)
            where TMessage : IDelegatingMessage
        {
            Receive<TMessage>(m => TryContinue(m, name, process));
        }

        protected virtual void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IReporterMessage
        {
            Log.Info("Enter Process {Name}", name);
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Listner);

            try
            {
                process(msg, reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                reporter.Compled(Operations.OperationResult.Failure(e.Unwrap()?.Message ?? "Unkowen"));
            }
        }

        protected virtual void TryContinue<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IDelegatingMessage
        {
            Log.Info("Enter Process {Name}", name);
            try
            {
                process(msg, msg.Reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                msg.Reporter.Compled(Operations.OperationResult.Failure(e.Unwrap()?.Message ?? "Unkowen"));
            }
        }
    }
}