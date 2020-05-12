using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    public sealed class DeferredSource : ModelConnectorBase<DeferredSource>, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private object? _value;
        private bool _hasErrors;
        private string? _error;

        protected override  void PropertyChangedHandler(PropertyChangedEvent msg)
        {
            if(Equals(_value, msg.Value)) return;
            _value = msg.Value;
            OnPropertyChanged(nameof(Value));
        }

        protected override void ValidateCompled(ValidatingEvent msg)
        {
            _error = msg.Reason;
            HasErrors = msg.Error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors
        {
            get => _hasErrors;
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
                Model?.Tell(new SetValue(Name, value));
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable GetErrors(string propertyName)
        {
            yield return _error;
        }

        public DeferredSource(string name, DataContextPromise promise) 
            : base(name, promise)
        {
        }
    }
}