using System.Diagnostics;
using Akka.Dispatch;

namespace Akka.MGIHelper.Core.Bus
{
    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternal : MessageDispatcher
    {
        public CallingThreadDispatcherInternal(MessageDispatcherConfigurator configurator)
            : base(configurator)
        {

        }

        protected override void ExecuteTask(IRunnable run)
            => run.Run();

        protected override void Shutdown()
        { }
    }
}