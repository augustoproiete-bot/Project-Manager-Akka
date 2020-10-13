using System;

namespace Tauron.Application.Wpf.Model
{
    public abstract class UIPropertyBase
    {
        private bool _isSetLocked;
        
        protected UIPropertyBase(string name)
        {
            Name = name;
            IsValid = QueryProperty.Create<bool>(a => IsValidSetter = a);
        }

        internal Action<bool> IsValidSetter { get; private set; } = b => { };

        public string Name { get; }

        public IQueryProperty<bool> IsValid { get; }

        protected internal object? InternalValue { get; internal set; }
        internal Func<object?, string?>? Validator { get; set; }

        public event Action? PropertyValueChanged;

        internal event Action? PriorityChanged;

        internal UIPropertyBase LockSet()
        {
            _isSetLocked = true;
            return this;
        }

        protected internal void SetValue(object? value)
        {
            if (_isSetLocked) return;

            InternalValue = value;
            OnPropertyValueChanged();
        }

        private void OnPropertyValueChanged()
        {
            PriorityChanged?.Invoke();
            PropertyValueChanged?.Invoke();
        }
    }
}