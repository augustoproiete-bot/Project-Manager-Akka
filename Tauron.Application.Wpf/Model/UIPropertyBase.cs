using System;

namespace Tauron.Application.Wpf.Model
{
    public abstract class UIPropertyBase
    {
        private bool _isSetLocked;

        public string Name { get; }

        protected internal object? InternalValue { get; internal set; }
        internal Func<object?, string?>? Validator { get; set; }

        public event Action? PropertyValueChanged;

        internal event Action? PriorityChanged;

        internal void LockSet() => _isSetLocked = true;

        protected internal void SetValue(object? value)
        {
            if(_isSetLocked) return;

            InternalValue = value;
            OnPropertyValueChanged();
        }

        protected UIPropertyBase(string name) => Name = name;

        private void OnPropertyValueChanged()
        {
            PriorityChanged?.Invoke();
            PropertyValueChanged?.Invoke();
        }
    }
}