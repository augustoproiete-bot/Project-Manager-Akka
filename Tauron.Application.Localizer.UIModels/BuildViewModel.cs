using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Workshop;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class BuildViewModel : UiActor
    {
        public BuildViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, ProjectFileWorkspace workspace) 
            : base(lifetimeScope, dispatcher)
        {
            #region Enable

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(false);
            this.RespondOnEventSource(workspace.Source.ProjectReset, r =>
                                                                     {
                                                                         IsEnabled += true;
                                                                         Importintegration += r.ProjectFile.BuildInfo.IntigrateProjects;
                                                                     });

            #endregion

            #region Import Integration

            Importintegration = RegisterProperty<bool>(nameof(Importintegration))
               .WithDefaultValue(true).ThenFlow(b => new ChangeIntigrate(b), this)
               .To.Mutate(workspace.Build).With(bm => bm.Intigrate, bm => ci => bm.SetIntigrate(ci.ToIntigrate)).ToSelf()
               .Then.Action(ii => Importintegration += ii.IsIntigrated)
               .AndReturn();

            #endregion
        }

        public UIProperty<bool> IsEnabled { get; private set; }

        public UIProperty<bool> Importintegration { get; private set; }

        private sealed class ChangeIntigrate
        {
            public bool ToIntigrate { get; }

            public ChangeIntigrate(bool intigrate) => ToIntigrate = intigrate;
        }
    }
}