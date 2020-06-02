using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class ElementAcessor
    {
        private ConfigurationElement _element;

        public ElementAcessor(ConfigurationElement element)
        {
            _element = element;
        }

        public IEnumerable<KeyValuePair<string, ConfigurationElement>> ToAddElements => _element.ToAddElements;

        public TType Set<TType>(TType value, string name)
        {
            return _element.Set(value, name);
        }

        public TType Get<TType>(string name)
        {
            return _element.Get<TType>(name);
        }

        public bool ContainsProperty(string name)
        {
            return _element.ContainsProperty(name);
        }

        public TType GetOrAdd<TType>(string name, Func<TType> fac)
        {
            return _element.GetOrAdd(name, fac);
        }

        public TType GetAddElement<TType>(string name)
            where TType : ConfigurationElement, new()
        {
            return _element.GetAddElement<TType>(name);
        }

        public TType? TryGetMergeElement<TType>()
            where TType : ConfigurationElement
        {
            return _element.TryGetMergeElement<TType>();
        }

        public TType GetMergeElement<TType>()
            where TType : ConfigurationElement, new()
        {
            return _element.GetMergeElement<TType>();
        }

        public void ReplaceMerge<TType>(TType? target)
            where TType : ConfigurationElement
        {
            _element.ReplaceMerge(target);
        }
    }
}