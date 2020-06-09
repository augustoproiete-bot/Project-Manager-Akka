using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class BuildViewModel : UiActor
    {
        public BuildViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
        }
    }
}