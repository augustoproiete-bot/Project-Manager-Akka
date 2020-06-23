using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    public sealed class DeferredSource : ModelConnectorBase<DeferredSource>, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string? _error;
        private bool _hasErrors;
        private object? _value;

        public DeferredSource(string name, DataContextPromise promise)
            : base(name, promise)
        {
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

        public IEnumerable GetErrors(string propertyName)
        {
            yield return _error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected override void PropertyChangedHandler(PropertyChangedEvent msg)
        {
            if (Equals(_value, msg.Value)) return;
            _value = msg.Value;
            OnPropertyChanged(nameof(Value));
        }

        protected override void NoDataContextFound()
        {
            Log.Debug("No DataContext Found for {Name}", Name);
        }

        protected override void ValidateCompled(ValidatingEvent msg)
        {
            _error = msg.Reason;
            HasErrors = msg.Error;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}