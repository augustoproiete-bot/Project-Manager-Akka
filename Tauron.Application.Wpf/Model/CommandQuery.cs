using System;

namespace Tauron.Application.Wpf.Model
{
    public abstract class CommandQuery
    {
        public abstract void Monitor(Action<bool> waiter);
    }
}