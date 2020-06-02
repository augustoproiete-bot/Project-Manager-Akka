using System.Diagnostics;
using System.Threading;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class TrackedProcessActor : ReceiveActor
    {
        private readonly Timer _exitCheck;
        private readonly int _id;

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly string _processName;
        private readonly Process _target;

        public TrackedProcessActor(Process process)
        {
            _target = process;

            _processName = _target.ProcessName;
            _id = process.Id;
            _log.Info("Track Process {Name} Startet: {Id}", _processName, _id);

            var self = Self;
            _exitCheck = new Timer(o => self.Tell(new InternalCheckProcess()), null, 1000, 1000);

            Receive<InternalProcessExit>(ProcessOnExited);
            Receive<InternalCheckProcess>(CheckProcess);
        }

        private void CheckProcess(InternalCheckProcess obj)
        {
            if (_target.HasExited)
                Self.Tell(new InternalProcessExit());
        }

        private void ProcessOnExited(InternalProcessExit msg)
        {
            _log.Info("Track Process {Name} Exited: {Id}", _processName, _id);

            Context.Parent.Tell(new ProcessExitMessage(_target, _processName, _id));
            Context.Stop(Self);
        }


        protected override void PostStop()
        {
            _exitCheck.Dispose();
            _target.Dispose();
        }

        private sealed class InternalProcessExit
        {
        }

        private sealed class InternalCheckProcess
        {
        }
    }
}