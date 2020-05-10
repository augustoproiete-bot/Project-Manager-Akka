using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    public sealed class DeferredSource : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly string _name;
        private IViewModel _model = null!;
        private readonly ILogger _log = Log.ForContext<DeferredSource>();

        private object? _value;
        private bool _hasErrors;
        private int _isInitializing = 1;
        private string? _error;
        private IEventActor? _eventActor;

        public DeferredSource(string name, DataContextPromise promise)
        {
            _name = name;

            promise
               .OnContext(model =>
                          {
                              _model = model;

                              if (model.IsInitialized)
                                  Task.Run(async () => await InitAsync());
                              else
                              {
                                  void OnModelOnInitialized()
                                  {
                                      Task.Run(async () => await InitAsync());
                                      _model.Initialized -= OnModelOnInitialized;
                                  }

                                  model.Initialized += OnModelOnInitialized;
                              }
                          });
        }

        private async Task InitAsync()
        {
            try
            {
                var (_, value) = await _model.Ask<GetValueResponse>(new GetValueRequest(_name), TimeSpan.FromSeconds(5));
                if (value != null)
                    Interlocked.Exchange(ref _value, value);

                //_log.Information("Ask For {Property}", _name);
                var eventActor = await _model.Ask<IEventActor>(new MakeEventHook(), TimeSpan.FromSeconds(5));
                //_log.Information("Ask Compled For {Property}", _name);

                eventActor.Register(HookEvent.Create<PropertyChangedEvent>(PropertyChangedHandler));
                eventActor.Register(HookEvent.Create<ValidatingEvent>(ValidateCompled));

                _model.Tell(new TrackPropertyEvent(_name), eventActor.OriginalRef);

                Interlocked.Exchange(ref _eventActor, eventActor);
                Interlocked.Exchange(ref _isInitializing, 0);

                OnPropertyChanged(nameof(Value));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
            }
            catch (Exception e)
            {
                _log.Error(e, "Error Bind Property {Name}", _name);
            }
        }

        private void PropertyChangedHandler(PropertyChangedEvent msg)
        {
            _value = msg.Value;
            OnPropertyChanged(nameof(Value));
        }

        private void ValidateCompled(ValidatingEvent msg)
        {
            _error = msg.Reason;
            HasErrors = msg.Error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors
        {
            get => _isInitializing == 1 || _hasErrors;
            private set
            {
                if (value == _hasErrors) return;
                _hasErrors = value;
                OnPropertyChanged();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
            }
        }

        public object? Value
        {
            get => _value;
            set
            {
                _value = value;
                _model?.Tell(new SetValue(_name, value));
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable GetErrors(string propertyName)
        {
            if (_isInitializing == 1) yield return "Initializing...";
            yield return _error;
        }
    }
}