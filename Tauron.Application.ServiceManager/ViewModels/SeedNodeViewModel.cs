using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class SeedUrlModel : ObservableObject
    {
        public string Url { get; }


        public SeedUrlModel(string url) => Url = url;

        public static SeedUrlModel New(string url)
            => new SeedUrlModel(url);
    }

    [UsedImplicitly]
    public class SeedNodeViewModel : StateUIActor
    {
        public SeedNodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker invoker) 
            : base(lifetimeScope, dispatcher, invoker)
        {
            #region Init

            Models = this.RegisterUiCollection<SeedUrlModel>(nameof(Models))
               .AndAsync();

            GetState<SeedState>().Query(EmptyQuery.Instance)
               .ContinueWith(c =>
                {
                    if(c.IsCompletedSuccessfully && c.Result != null)
                        Models.AddRange(c.Result.Seeds.Select(SeedUrlModel.New));

                    Log.Warning("Seed Query for Intitial Data failed");
                });
               

            DispatchAction(new TryJoinAction(), false);

            #endregion;

            #region Add Seed

            void AddSeedEntry(AddSeedUrlEvent entry)
            {
                entry.SeedUrl.WhenNotEmpty(s =>
                                       {
                                           Log.Info("Add Seed Node to List {URL}", s);
                                           Models.Add(
                                               new SeedUrlModel(s),
                                               c =>
                                               {
                                                   try
                                                   {
                                                       Cluster.Get(Context.System).Join(Address.Parse(c.Url));
                                                   }
                                                   catch (Exception e)
                                                   {
                                                       Log.Error(e, "Faild to Join {Url}", c.Url);
                                                   }
                                               });
                                       });
            }

            WhenStateChanges<SeedState>().FromEvent(s => s.AddSeed).ToAction(AddSeedEntry);
            
            NewCommad
               .ThenFlow(
                    this.ShowDialog<IAddSeedUrlDialog, DialogSeedEntry, IEnumerable<DialogSeedEntry>>(() => Models.Select(m => new DialogSeedEntry(m.Url))),
                    b => b.Action(e => DispatchAction(new AddSeedUrlAction(e.Url))))
               .ThenRegister("AddSeedUrl");

            #endregion

            #region Remove Seed

            void DoRemove(RemoveSeedUrlEvent evt)
            {
                Log.Info("Removing Seed {URL}", evt.SeedUrl);
                Models.Remove(Models.First(m => m.Url == evt.SeedUrl));
            }

            SelectIndex = RegisterProperty<int>(nameof(SelectIndex));

            NewCommad
               .WithCanExecute(b => b.FromProperty(SelectIndex, i => i > -1))
               .ToStateAction(() => new RemoveSeedUrlAction(Models.ElementAt(SelectIndex).Url))
               .ThenRegister("RemoveSeed");

            WhenStateChanges<SeedState>().FromEvent(e => e.RemoveSeed)
               .ToAction(DoRemove);

            #endregion
        }

        public UICollectionProperty<SeedUrlModel> Models { get; }

        public UIProperty<int> SelectIndex { get; }
    }
}