using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class BelogsToStateAttribute : Attribute
    {
        public Type StateType { get; }

        public BelogsToStateAttribute(Type stateType) 
            => StateType = stateType;
    }
}