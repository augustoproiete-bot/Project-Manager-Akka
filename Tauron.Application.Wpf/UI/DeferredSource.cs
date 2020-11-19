using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.UI
{
    public sealed class DeferredSource : ModelConnectorBase<DeferredSource>, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private Maybe<string>  _error;
        private Maybe<bool>    _hasErrors;
        private Maybe<object?> _value;

        public DeferredSource(string name, Maybe<DataContextPromise> promise)
            : base(name, promise)
        {
        }

        public object? Value
        {
            get => _value;
            set
            {
                _value = May(value);
                Do(from model in Model
                   select Action(() => Tell(model.Actor, new SetValue(Name, _value))));
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors
        {
            get => _hasErrors.OrElseDefault();
            private set
            {
                if (value == _hasErrors.OrElseDefault()) return;
                _hasErrors = May(value);
                OnPropertyChanged();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
            }
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            yield return _error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected override void PropertyChangedHandler(PropertyChangedEvent msg)
        {
            _value = Either(RunWith(() =>
                                        from newVal in msg.Value
                                        where !Equals(newVal, _value.OrElseDefault())
                                        select newVal, () => OnPropertyChanged(nameof(Value)))
                          , _value);
        }

        protected override void NoDataContextFound()
        {
            Log.Debug("No DataContext Found for {Name}", Name);
        }

        protected override void ValidateCompled(ValidatingEvent msg)
        {
            _error    = msg.Reason;
            HasErrors = msg.Error;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}