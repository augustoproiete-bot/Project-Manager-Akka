using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.FanControl;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.UI
{
    public class AutoFanControlModel : INotifyPropertyChanged, IAsyncDisposable
    {
        private readonly FanControl _fanControl;
        
        private TrackingEvent _trackingEvent;
        private bool _fanRunning;

        public TrackingEvent TrackingEvent
        {
            get => _trackingEvent;
            set
            {
                if (Equals(value, _trackingEvent)) return;
                _trackingEvent = value;
                OnPropertyChanged();
            }
        }

        public bool FanRunning
        {
            get => _fanRunning;
            set
            {
                if (value == _fanRunning) return;
                _fanRunning = value;
                OnPropertyChanged();
            }
        }

        public FanControlOptions Options { get; }

        public AutoFanControlModel()
        {
            Options = new FanControlOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? throw new InvalidOperationException(), "FanSettings.json"));

            _fanControl = new FanControl(Options);
            _fanControl.DataRecieved += e =>
            {
                TrackingEvent = e;
                return Task.CompletedTask;
            };
            _fanControl.FanStatus += e =>
            {
                FanRunning = e;
                return Task.CompletedTask;
            };
        }

        public async Task Init()
        {
            await Options.Load();
            _fanControl.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ValueTask DisposeAsync() => _fanControl.DisposeAsync();
    }
}