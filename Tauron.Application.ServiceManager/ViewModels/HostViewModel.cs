using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    [UsedImplicitly]
    public sealed class HostViewModel : UiActor
    {
        public HostViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer) 
            : base(lifetimeScope, dispatcher)
        {
            var eventSystem = Context.System.EventStream;
            var showApps = new SimpleCommand(o =>
            {
                if (o != null) 
                    eventSystem.Publish(new DisplayApplications((string) o));
            });

            HostApi hostConnector = HostApi.CreateOrGet(Context.System);

            var commandExecutor = Context.ActorOf(Props.Create<CommandExutor>(), "HostCommand-Executor");

            HostEntries = this.RegisterUiCollection<UIHostEntry>(nameof(HostEntries)).AndAsync();

            Flow<HostEntryChanged>(b =>
            {
                b.Action(he =>
                {
                    var entry = HostEntries.FirstOrDefault(e => e.ActorPath == he.Path);
                    if (he.Removed)
                    {
                        if (entry == null) return;
                        
                        Log.Info("Removing Host Entry {Name}", entry.Name);
                        HostEntries.Remove(entry);
                    }
                    else
                    {
                        if (entry == null)
                        {
                            Log.Info("Addinf Host Entry {Path}", he.Path);
                            HostEntries.Add(new UIHostEntry(he.Path, he.Name, showApps, localizer, hostConnector, commandExecutor,
                                InvalidateRequerySuggested, this, hostConnector));
                        }
                        else
                        {
                            Log.Info("Changing Host Name {Name} {Path}", he.Name, he.Path);
                            entry.Name = he.Name;
                        }
                    }
                });
            });

            AddResource(hostConnector.Event<HostEntryChanged>());
        }

        public UICollectionProperty<UIHostEntry> HostEntries { get; }
    }

    public sealed class UIHostEntry : ObservableObject
    {
        private string? _name;
        private readonly LocLocalizer _localizer;
        private readonly HostApi _hostApi;
        private readonly UiActor _actor;
        private readonly HostApi _api;

        public string Name
        {
            get => string.IsNullOrWhiteSpace(_name) ? ActorPath.ToString() : _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public ActorPath ActorPath { get; }

        public ICommand Applications { get; }

        public ICollection<HostCommand> HostCommands { get; }

        public UIHostEntry(ActorPath actorPath, string name, ICommand applications, LocLocalizer localizer, HostApi hostApi, 
            IActorRef comandExecute, Action commandFinish, UiActor actor, HostApi api)
        {
            HostCommand BuildCommand(string commandName, Func<Task<bool>> exec) 
                => new HostCommand(commandName, localizer, exec, comandExecute, commandFinish);

            ActorPath = actorPath;
            _name = name;
            _localizer = localizer;
            _hostApi = hostApi;
            _actor = actor;
            _api = api;
            Applications = applications;

            var commandLocal = localizer.HostCommand;

            HostCommands = new List<HostCommand>
                           {
                                BuildCommand(commandLocal.CommandNameStopAll, async () => await RunCommand(new StopAllApps(Name), () => ConfirmAll(true))),
                                BuildCommand(commandLocal.CommandNameStopApp, async () => await SelectName(true, async s => await RunCommand(new StopHostApp(Name, s)))),
                                BuildCommand(commandLocal.CommandNameStartAll, async () => await RunCommand(new StartAllApps(Name), () => ConfirmAll(false))),
                                BuildCommand(commandLocal.CommandNameStartApp, async () => await SelectName(false, async s => await RunCommand(new StartHostApp(Name, s))))
                           };
        }

        private async Task<bool> SelectName(bool stop, Func<string, Task<bool>> continueWhith)
        {
            var app = _actor.ShowDialog<ISelectHostAppDialog, HostApp?, Task<HostApp[]>>(() =>
                _api.QueryApps(Name)
                   .ContinueWith(t => t.Result.Where(e => e.Running != stop).ToArray()))();

            if (app == null) return false;
            return await continueWhith(app.Name);
        }

        private async Task<bool> RunCommand(InternalHostMessages.CommandBase msg, Func<Task<bool>>? confirm = null)
        {
            if(confirm == null || await confirm())
                return (await _hostApi.ExecuteCommand(msg)).Success;
            return false;
        }

        private async Task<bool> ConfirmAll(bool stop)
        {
            var coordinator = DialogCoordinator.Instance;
            if (stop)
                return await coordinator.ShowMessage(_localizer.HostCommand.DialogCommandTitle, _localizer.HostCommand.DiaologCommandStopAll, null) == true;
            return await coordinator.ShowMessage(_localizer.HostCommand.DialogCommandTitle, _localizer.HostCommand.DialogCommandStartAll, null) == true;
        }


    }

    public sealed class HostCommand : ObservableObject, ICommandTask
    {
        private readonly LocLocalizer _localizer;
        private readonly Func<Task<bool>> _executor;
        private readonly Action _commadFinish;

        private string? _status;
        private bool _running;

        public string Name { get; }

        public string? Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public ICommand Start { get; }

        public HostCommand(string name, LocLocalizer localizer, Func<Task<bool>> executor, IActorRef commandExecutor, Action commadFinish)
        {
            _localizer = localizer;
            _executor = executor;
            _commadFinish = commadFinish;
            Name = name;

            Start = new SimpleCommand(() => !_running, () =>
            {
                _running = true;
               commandExecutor.Tell(this); 
            });
        }

        public Task<bool> Run()
        {
            Status = _localizer.HostCommand.Running;
            return _executor();
        }

        public void ReportError(Exception? e)
        {
            Status = e == null ? _localizer.HostCommand.UnkowenError : e.Message;

            _running = false;
            _commadFinish();
        }

        public void Finish()
        {
            Status = _localizer.HostCommand.Finish;

            _running = false;
            _commadFinish();
        }
    }
}