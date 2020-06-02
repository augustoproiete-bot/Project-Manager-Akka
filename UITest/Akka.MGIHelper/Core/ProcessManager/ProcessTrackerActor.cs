using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessTrackerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Timer _processUpdater;
        private ImmutableArray<string> _tracked = ImmutableArray<string>.Empty;

        public ProcessTrackerActor()
        {
            //_processUpdater = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(5000, 5000, Self, new GatherProcess(), ActorRefs.NoSender);
            var self = Self;
            _processUpdater = new Timer(o => self.Tell(new GatherProcess()), null, 5000, 5000);

            Receive<GatherProcess>(GetProcesses);
            Receive<ProcessExitMessage>(ProcessExit);
            Receive<RegisterProcessFile>(Track);
        }

        private void ProcessExit(ProcessExitMessage obj)
        {
            Context.Parent.Tell(new ProcessStateChange(ProcessChange.Stopped, obj.Name, obj.Id, obj.Target));
        }

        private string FormatName(int id)
        {
            return $"Process-{id}";
        }

        private void GetProcesses(GatherProcess state)
        {
            try
            {
                if (Context.GetChildren().Count() == _tracked.Length)
                    return;

                _log.Info("Update Processes");
                foreach (var process in Process.GetProcesses())
                    try
                    {
                        if (!Context.Child(FormatName(process.Id)).Equals(ActorRefs.Nobody))
                        {
                            process.Dispose();
                            continue;
                        }

                        var processName = process.ProcessName;
                        if (!_tracked.Any(s => s.Contains(processName)))
                        {
                            process.Dispose();
                            continue;
                        }

                        _log.Info("Process Found {Name}", process.ProcessName);
                        Context.ActorOf(Props.Create(() => new TrackedProcessActor(process)), FormatName(process.Id));
                        Context.Parent.Tell(new ProcessStateChange(ProcessChange.Started, processName, process.Id, process));
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, "Error on Check Process");
                    }
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on Get Processes");
            }
        }

        private void Track(RegisterProcessFile msg)
        {
            var fileName = msg.FileName.Trim();
            if (string.IsNullOrWhiteSpace(fileName)) return;
            _tracked = _tracked.Add(fileName);
        }

        protected override void PostStop()
        {
            _processUpdater.Dispose();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(exception => Directive.Stop);
        }
    }
}