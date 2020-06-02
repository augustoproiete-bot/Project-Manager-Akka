using System;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class FluentPropertyRegistration<TData>
    {
        internal FluentPropertyRegistration(string name, UiActor actor)
        {
            Property = new UIProperty<TData>(name);
            actor.RegisterProperty(Property);
        }

        public UIProperty<TData> Property { get; }

        public FluentPropertyRegistration<TData> WithValidator(string name, Func<TData, string> validator)
        {
            Property.Validator = o =>
            {
                if (o is TData value)
                    return validator(value);
                return null;
            };

            return this;
        }

        public FluentPropertyRegistration<TData> WithDefaultValue(TData data)
        {
            Property.InternalValue = data;
            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action changed)
        {
            Property.PropertyValueChanged += changed;
            return this;
        }

        public static implicit operator UIProperty<TData>(FluentPropertyRegistration<TData> config)
        {
            return config.Property;
        }
    }
}