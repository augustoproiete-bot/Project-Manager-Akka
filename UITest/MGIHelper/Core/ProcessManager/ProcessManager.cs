using System;
using System.Collections.Concurrent;

namespace MGIHelper.Core.ProcessManager
{
    public sealed class ProcessManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, ITargetProcess> _targetProcesses = new ConcurrentDictionary<string, ITargetProcess>();
        private readonly ProcessTracker _processTracker = new ProcessTracker();

        public ProcessManager()
        {
            _processTracker.Started += ProcessTrackerOnStarted;
            _processTracker.Stoped += ProcessTrackerOnStoped;
        }

        private ITargetProcess? TryGet(string name) 
            => _targetProcesses.TryGetValue(name, out var targetProcess) ? targetProcess : null;

        private void ProcessTrackerOnStoped(ProcessTracker.TrackedProcess obj) 
            => TryGet(obj.ProcessName)?.Exit(obj.Target);

        private void ProcessTrackerOnStarted(ProcessTracker.TrackedProcess obj) 
            => TryGet(obj.ProcessName)?.Found(obj.Target);

        public void Register(ITargetProcess script)
        {
            foreach (var fileName in script.FileNames)
            {
                if(string.IsNullOrWhiteSpace(fileName)) continue;
                if(_targetProcesses.ContainsKey(fileName))
                    throw new InvalidOperationException($"Only One Scrip per File Suporrtet: {script}");
                
                _targetProcesses[fileName] = script;
                _processTracker.Track(fileName);
            }
        }

        public void Dispose() 
            => _processTracker.Dispose();
    }
}