using System;

namespace Akkatecture.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TagAttribute : Attribute
    {
        public string Name { get; }

        public TagAttribute(string name) => Name = name;
    }
}