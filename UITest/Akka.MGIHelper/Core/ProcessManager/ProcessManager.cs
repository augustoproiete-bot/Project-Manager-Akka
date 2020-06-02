using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly IActorRef _processTracker;
        private ImmutableDictionary<string, IActorRef> _targetProcesses = ImmutableDictionary<string, IActorRef>.Empty;

        public ProcessManagerActor()
        {
            _processTracker = Context.ActorOf<ProcessTrackerActor>();

            Receive<RegisterProcessList>(Register);
            Receive<ProcessStateChange>(ProcessStateChange);
        }

        private void ProcessStateChange(ProcessStateChange obj)
        {
            var (key, target) = _targetProcesses.FirstOrDefault(p => p.Key.Contains(obj.Name));
            if (string.IsNullOrWhiteSpace(key)) return;

            target.Tell(obj);
        }

        private void Register(RegisterProcessList script)
        {
            foreach (var fileName in script.Files.Where(fileName => !string.IsNullOrWhiteSpace(fileName)))
            {
                if (_targetProcesses.ContainsKey(fileName))
                    _log.Error("Only One Scrip per File Suporrtet: {Script}", script.Intrest.Path.ToString());

                _targetProcesses = _targetProcesses.SetItem(fileName, script.Intrest);
                _processTracker.Tell(new RegisterProcessFile(fileName));
            }
        }
    }
}