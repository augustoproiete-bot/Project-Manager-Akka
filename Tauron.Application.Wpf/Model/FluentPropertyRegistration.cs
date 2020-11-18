using System;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class FluentPropertyRegistration<TData>
    {
        internal FluentPropertyRegistration(string name, IUiActor actor)
        {
            Property = new UIProperty<TData>(name);
            actor.RegisterProperty(Property);
        }

        public UIProperty<TData> Property { get; }

        public FluentPropertyRegistration<TData> WithValidator(Func<TData, string?> validator)
        {
            Maybe<string> Executor(Maybe<object> mayData)
            {
                return from data in mayData
                    where data is TData
                    select MayNotEmpty(validator((TData)data));
            }

            Property.Validator = new Func<Maybe<object>, Maybe<string>>(Executor).ToMaybe();

            return this;
        }

        public FluentPropertyRegistration<TData> WithDefaultValue(TData data)
        {
            Property.InternalValue = MayNotNull(data as object)!;
            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action changed)
        {
            Property.PropertyValueChanged += changed;
            return this;
        }

        public FluentPropertyRegistration<TData> OnChange(Action<TData> changed)
        {
            Property.PropertyValueChangedFunc += changed;
            return this;
        }


        public static implicit operator UIProperty<TData>(FluentPropertyRegistration<TData> config) => config.Property;
    }
}