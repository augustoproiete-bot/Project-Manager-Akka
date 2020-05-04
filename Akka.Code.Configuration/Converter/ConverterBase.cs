using System;
using Akka.Code.Configuration.Elements;

namespace Akka.Code.Configuration.Converter
{
    public abstract class ConverterBase
    {
        public static ConverterBase Find(Type type)
        {
            if (type == typeof(TimeSpan))
                return TimeSpanConverter.Converter;
            if (type == typeof(bool))
                return BoolConverter.Instance;
            if (type.BaseType == typeof(Enum))
                return EnumConverter.Converter;
            if (type.IsAssignableFrom(typeof(ConfigurationElement)))
                return ConfigurationElementConverter.Instance;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ArrayElement<>))
                return ArrayConverter.Instance;

            return GenericConverter.Converter;
        }

        public abstract string? ToElementValue(object? obj);
    }
}