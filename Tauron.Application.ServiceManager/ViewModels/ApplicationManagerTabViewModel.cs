﻿using System.Windows.Threading;
using Autofac;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ApplicationManagerTabViewModel : StateUIActor
    {
        public ApplicationManagerTabViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker actionInvoker) 
            : base(lifetimeScope, dispatcher, actionInvoker)
        {
        }
    }
}