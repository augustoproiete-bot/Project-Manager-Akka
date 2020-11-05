using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class ProcessorAttribute : Attribute
    {
        
    }
}