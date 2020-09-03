using System;

namespace Tauron.Application.Wpf.Model
{
    public abstract class UIPropertyBase
    {
        private bool _isSetLocked;

        protected UIPropertyBase(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool IsValid { get; internal set; }

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