using System;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed record HookEvent(Delegate Invoker, Type Target)
    {
        public static HookEvent Create<TType>(Action<TType> action) 
            => new(action, typeof(TType));
    }
}