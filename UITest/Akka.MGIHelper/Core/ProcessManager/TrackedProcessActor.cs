using System.Diagnostics;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class TrackedProcessActor : ReceiveActor
    {
        private sealed class InternalProcessExit
        {
            
        }

        private sealed class InternalCheckProcess
        {
            
        }

        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly ICancelable _exitCheck;
        private readonly int _id;
        private readonly string _processName;
        private readonly Process _target;

        public TrackedProcessActor(Process process)
        {
            _target = process;

            _processName = _target.ProcessName;
            _id = process.Id;
            _log.Info("Track Process {Name} Startet: {Id}", _processName, _id);

            _exitCheck = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(1000, 1000, Self, new InternalCheckProcess(), Self);

            Receive<InternalProcessExit>(ProcessOnExited);
            Receive<InternalCheckProcess>(CheckProcess);
        }

        private void CheckProcess(InternalCheckProcess obj)
        {
            if(_target.HasExited)
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
            _exitCheck.Cancel();
            _target.Dispose();
        }
    }
}