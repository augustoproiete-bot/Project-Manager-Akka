using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [PublicAPI]
    [BaseTypeRequired(typeof(IMiddleware))]
    public sealed class MiddlewareAttribute : Attribute
    {
        
    }
}