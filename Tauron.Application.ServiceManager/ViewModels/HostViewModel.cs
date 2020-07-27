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
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    [UsedImplicitly]
    public sealed class HostViewModel : UiActor
    {
        private readonly IActorRef _hostConnector;

        private EventSubscribtion _eventSubscribtion = EventSubscribtion.Empty;

        public HostViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer) 
            : base(lifetimeScope, dispatcher)
        {
            _hostConnector = HostApi.Create(Context);

            var commandExecutor = Context.ActorOf(Props.Create<CommandExutor>(), "HostCommand-Executor");

            HostEntries = this.RegisterUiCollection<UIHostEntry>(nameof(HostEntries)).AndAsync();

            this.Flow<HostEntryChanged>()
                .From.Action(he =>
                {
                    var entry = HostEntries.FirstOrDefault(e => e.ActorPath == he.Path);
                    if (he.Removed)
                    {
                        if(entry != null)
                            HostEntries.Remove(entry);
                    }
                    else
                    {
                        if (entry == null)
                            HostEntries.Add(new UIHostEntry(he.Path, he.Name, SimpleCommand.Empty, localizer, _hostConnector, commandExecutor, CommandChanged));
                        else
                            entry.Name = he.Name;
                    }
                });
        }

        protected override void PreStart()
        {
            _eventSubscribtion = _hostConnector.SubscribeToEvent<HostEntryChanged>();
            base.PreStart();
        }

        protected override void PostStop()
        {
            _eventSubscribtion.Dispose();
            base.PostStop();
        }

        public UICollectionProperty<UIHostEntry> HostEntries { get; }
    }

    public sealed class UIHostEntry : ObservableObject
    {
        private string? _name;
        private readonly IActorRef _hostApi;

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

        public UIHostEntry(ActorPath actorPath, string name, ICommand applications, LocLocalizer localizer, IActorRef hostApi, IActorRef comandExecute, Action commandFinish)
        {
            ActorPath = actorPath;
            _name = name;
            _hostApi = hostApi;
            Applications = applications;

            HostCommands = new List<HostCommand>
                           {
                                new HostCommand(localizer.HostView.HostCommandStopAll, localizer,
                                    async () => await RunCommand(new StopAllApps(Name)), 
                                    comandExecute, commandFinish)
                           };
        }

        private async Task<bool> RunCommand(object msg)
            => (await _hostApi.Ask<OperationResponse>(msg, TimeSpan.FromMinutes(2))).Success;
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