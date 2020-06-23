﻿using System.Windows.Threading;
using Autofac;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            NodeView = this.RegisterModel<NodeViewModel>(nameof(NodeView), "Node-View");
            SeedView = this.RegisterModel<SeedNodeViewModel>(nameof(SeedView), "Seed-View");
        }

        public ModelProeprty NodeView { get; }

        public ModelProeprty SeedView { get; }
    }
}