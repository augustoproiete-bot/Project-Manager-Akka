﻿using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessTrackerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly ICancelable _processUpdater;
        private ImmutableArray<string> _tracked;

        public ProcessTrackerActor()
        {
            _processUpdater = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(5000, 5000, Self, new GatherProcess(), Self);

            Receive<GatherProcess>(GetProcesses);
            Receive<ProcessExitMessage>(ProcessExit);
            Receive<RegisterProcessFile>(Track);
        }

        private void ProcessExit(ProcessExitMessage obj) 
            => Context.Parent.Tell(new ProcessStateChange(ProcessChange.Stopped, obj.Name, obj.Id, obj.Target));

        private string FormatName(int id) => $"Process-{id}";

        private void GetProcesses(GatherProcess state)
        {
            _log.Info("Update Processes");
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (!Context.Child(FormatName(process.Id)).Equals(ActorRefs.Nobody))
                            continue;
                        
                        var processName = process.ProcessName;
                        if (!_tracked.Any(s => s.Contains(processName))) continue;

                        _log.Info("Process Found {Name}", process.ProcessName);
                        Context.ActorOf(Props.Create(() => new TrackedProcessActor(process)), FormatName(process.Id));
                        Context.Parent.Tell(new ProcessStateChange(ProcessChange.Started, processName, process.Id, process));
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, "Error on Check Process");
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on Get Processes");
            }

        }

        private void Track(RegisterProcessFile msg)
        {
            var fileName = msg.FileName;
            if(string.IsNullOrWhiteSpace(fileName)) return;
            _tracked = _tracked.Add(fileName);
        }

        protected override void PostStop() 
            => _processUpdater.Cancel();

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(5, 5000, exception => Directive.Stop);
    }
}