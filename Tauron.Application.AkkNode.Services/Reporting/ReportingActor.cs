using System;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services
{
    public abstract class ReportingActor : ExposedReceiveActor
    {
        protected virtual void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IReporterMessage
        {
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Listner);

            try
            {
                process(msg, reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                reporter.Compled(OperationResult.Failure(e.Unwrap().Message));
            }
        }

        protected virtual void TryContinue<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
            where TMessage : IDelegatingMessage
        {
            try
            {
                process(msg, msg.Reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
                msg.Reporter.Compled(OperationResult.Failure(e.Unwrap().Message));
            }
        }
    }
}