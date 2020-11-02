using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [PublicAPI]
    [MeansImplicitUse]
    public sealed class CacheAttribute : Attribute
    {
        public bool UseParent { get; set; }
    }
}