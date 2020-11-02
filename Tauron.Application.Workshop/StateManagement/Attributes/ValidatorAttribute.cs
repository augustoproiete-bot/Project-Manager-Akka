using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class ValidatorAttribute : Attribute
    {
        
    }
}