using System;
using JetBrains.Annotations;

namespace Akka.Code.Configuration.Elements
{
    [PublicAPI]
    public sealed class AkkaType
    {
        public AkkaType(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public static implicit operator AkkaType(string source)
        {
            return new AkkaType(source);
        }

        public static implicit operator AkkaType(Type source)
        {
            return new AkkaType(source.AssemblyQualifiedName);
        }

        public override string ToString()
        {
            return $"\"{Type}\"";
        }
    }
}