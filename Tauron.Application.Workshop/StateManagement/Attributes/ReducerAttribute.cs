using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public sealed class ReducerAttribute : Attribute
    {
        
    }
}