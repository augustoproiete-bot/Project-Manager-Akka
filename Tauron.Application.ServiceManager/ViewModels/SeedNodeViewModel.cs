using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public class SeedNodeViewModel : UiActor
    {
        public SeedNodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            #region Add Seed

            void AddSeedEntry(DialogSeedEntry entry)
            {

            }

            NewCommad
               .ThenFlow(this.ShowDialog<IAddSeedUrlDialog, DialogSeedEntry>())
               .From.Action(AddSeedEntry)
               .AndReturn().ThenRegister("AddSeedUrl");

            #endregion
        }
    }
}