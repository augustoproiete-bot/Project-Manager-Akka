using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MGIHelper.Core.ProcessManager
{
    public sealed class ProcessTracker : IDisposable
    {
        private readonly ConcurrentDictionary<int, TrackedProcess> _processes = new ConcurrentDictionary<int, TrackedProcess>();
        private readonly ConcurrentBag<string> _tracked = new ConcurrentBag<string>();
        private readonly Timer _processTimer;

        public event Action<TrackedProcess>? Started;

        public event Action<TrackedProcess>? Stoped; 

        public ProcessTracker() 
            => _processTimer = new Timer(GetProcesses, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

        private void GetProcesses(object? state)
        {
            try
            {

                foreach (var trackedProcess in _processes)
                    trackedProcess.Value.Checked = false;

                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (_processes.TryGetValue(process.Id, out var tp)) tp.Checked = true;
                        else
                        {
                            var processName = process.ProcessName;
                            if (!_tracked.Any(s => s.Contains(processName))) continue;

                            tp = new TrackedProcess(process, ProcessExited);
                            OnStarted(tp);
                            _processes[tp.Id] = tp;
                        }
                    }
                    catch 
                    {
                        continue;
                    }
                }

                foreach (var key in _processes.Where(tp => !tp.Value.Checked).Select(p => p.Key).ToArray())
                {
                    if (!_processes.TryRemove(key, out var p)) continue;
                    
                    OnStoped(p);
                    p.Dispose();
                }
            }
            catch (Exception e)
            {
                // All Ignored    
            }
            finally
            {
                _processTimer.Change(5000, Timeout.Infinite);
            }

        }

        private void ProcessExited(TrackedProcess obj)
        {
            if (!_processes.TryRemove(obj.Id, out var p)) return;

            OnStoped(p);
            p.Dispose();
        }

        public void Track(string fileName)
        {
            if(string.IsNullOrWhiteSpace(fileName)) return;
            _tracked.Add(fileName);
        }

        public void Dispose() 
            => _processTimer.Dispose();

        private void OnStoped(TrackedProcess obj) 
            => Stoped?.Invoke(obj);

        private void OnStarted(TrackedProcess obj) 
            => Started?.Invoke(obj);

        public class TrackedProcess : IDisposable
        {
            private readonly Action<TrackedProcess> _exited;

            public int Id { get; }

            public string ProcessName { get; }

            public Process Target { get; }

            public bool Checked { get; set; } = true;

            public TrackedProcess(Process process, Action<TrackedProcess> exited)
            {
                Target = process;
                Target.Exited += ProcessOnExited;
                Target.EnableRaisingEvents = true;
                _exited = exited;

                ProcessName = Target.ProcessName;
                Id = process.Id;
            }

            private void ProcessOnExited(object? sender, EventArgs e) 
                => _exited(this);

            public void Dispose()
            {
                Target.Exited -= ProcessOnExited;
                Target.Dispose();
            }
        }
    }
}