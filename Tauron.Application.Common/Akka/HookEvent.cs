using System;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class HookEvent
    {
        private HookEvent(Delegate invoker, Type target)
        {
            Invoker = invoker;
            Target = target;
        }

        public Delegate Invoker { get; }
        public Type Target { get; }

        public static HookEvent Create<TType>(Action<TType> action)
        {
            return new HookEvent(action, typeof(TType));
        }
    }
}