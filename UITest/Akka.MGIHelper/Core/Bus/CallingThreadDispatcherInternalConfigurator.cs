using System.Diagnostics;
using Akka.Configuration;
using Akka.Dispatch;

namespace Akka.MGIHelper.Core.Bus
{
    [DebuggerNonUserCode]
    public sealed class CallingThreadDispatcherInternalConfigurator : MessageDispatcherConfigurator
    {
        private readonly CallingThreadDispatcherInternal _dispatcher;

        public CallingThreadDispatcherInternalConfigurator(Config config, IDispatcherPrerequisites prerequisites) : base(config, prerequisites)
        {
            _dispatcher = new CallingThreadDispatcherInternal(this);
        }

        public override MessageDispatcher Dispatcher()
            => _dispatcher;
    }
}