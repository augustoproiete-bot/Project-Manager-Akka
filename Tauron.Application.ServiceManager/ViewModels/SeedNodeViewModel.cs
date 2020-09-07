using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
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
    public class SeedNodeViewModel : UiActor
    {
        public SeedNodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, AppConfig config, CommonAppInfo info) 
            : base(lifetimeScope, dispatcher)
        {
            #region Init

            void TryJoin()
            {
                info.ConnectionState = ConnectionState.Connecting;
                Log.Info("Trying Join to Cluster");
                var cluster = Cluster.Get(Context.System);

                if (cluster.State.Members.Count != 0)
                {
                    info.ConnectionState = ConnectionState.Online;
                    Log.Info("Cluster Joins is already Compled");
                    return;
                }

                var source = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                cluster.JoinSeedNodesAsync(Models.Select(m => Address.Parse(m.Url)), source.Token)
                   .ContinueWith(t =>
                                 {
                                     source.Dispose();
                                     info.ConnectionState = t.IsCompletedSuccessfully ? ConnectionState.Online : ConnectionState.Offline;
                                 });
            }

            Models = this.RegisterUiCollection<SeedUrlModel>(nameof(Models))
               .AndAsync()
               .AndInitialElements(config.SeedUrls.Select(SeedUrlModel.New));

            if(Models.Count > 0) 
                TryJoin();

            #endregion;

            #region Add Seed

            void AddSeedEntry(DialogSeedEntry entry)
            {
                entry.Url.WhenNotEmpty(s =>
                                       {
                                           Log.Info("Add Seed Node to List {URL}", s);
                                           config.SeedUrls = config.SeedUrls.Add(s);
                                           Models.Add(
                                               new SeedUrlModel(s),
                                               c => c.When(c => c == 1, TryJoin));
                                       });
            }

            NewCommad
               .ThenFlow(this.ShowDialog<IAddSeedUrlDialog, DialogSeedEntry,  IEnumerable<DialogSeedEntry>>(
                    () => Models.Select(m => new DialogSeedEntry(m.Url))))
               .From.Action(AddSeedEntry)
               .AndReturn().ThenRegister("AddSeedUrl");

            #endregion

            #region Remove Seed

            void DoRemove(SeedUrlModel model)
            {
                Log.Info("Removing Seed {URL}", model.Url);
                config.SeedUrls = config.SeedUrls.Remove(model.Url);
                Models.Remove(model);

                if (Models.Count == 0)
                    Cluster.Get(Context.System).LeaveAsync();
            }

            SelectIndex = RegisterProperty<int>(nameof(SelectIndex));

            NewCommad
               .WithCanExecute(() => SelectIndex > -1)
               .ThenFlow(() => Models.ElementAt(SelectIndex))
               .From.Action(DoRemove)
               .AndReturn()
               .ThenRegister("RemoveSeed");

            #endregion
        }

        public UICollectionProperty<SeedUrlModel> Models { get; }

        public UIProperty<int> SelectIndex { get; }
    }
}