using System;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Akka.MGIHelper.Settings.Provider
{
    public sealed class XmlProvider : ISettingProvider
    {
        public ImmutableDictionary<string, string> Load()
        {
            XElement ele = XElement.Load("ProcessConfig.xml");

            return ele.Elements()
                .Aggregate(ImmutableDictionary<string, string>.Empty, (current, xElement) => current.Add(xElement.Name.ToString(), xElement.Value));
        }

        public void Save(ImmutableDictionary<string, string> data)
        {
            throw new NotSupportedException("Xml does Not Support Saving");
        }
    }
}