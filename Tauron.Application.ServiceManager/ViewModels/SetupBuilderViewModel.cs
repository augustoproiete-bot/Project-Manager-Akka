using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.SetupBuilder;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public sealed class SetupBuilderViewModel : UiActor
    {
        private readonly AppConfig _config;
        private readonly HostApi _api;
        private readonly SetupServer _server;

        private EventSubscribtion? _subscribtion;

        public SetupBuilderViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, AppConfig config) 
            : base(lifetimeScope, dispatcher)
        {
            _config = config;
            _server = new SetupServer(s => UICall(() => TerminalLines.Add(s)), Context.System.Settings.Config);

            CurrentError = RegisterProperty<string>(nameof(CurrentError));
            AddSeed = RegisterProperty<bool>(nameof(AddSeed));
            TerminalLines = this.RegisterUiCollection<string>(nameof(TerminalLines)).AndAsync();

            var hostEntrys = new HashSet<string>();
            _api = HostApi.CreateOrGet(Context);
            Receive<HostEntryChanged>(e =>
            {
                if(string.IsNullOrWhiteSpace(e.Name))
                    return;

                if (e.Removed) hostEntrys.Remove(e.Name);
                else hostEntrys.Add(e.Name);
            });

            Func<string, string?> HostNameValidator(Func<UIProperty<string>> counterPart)
            {
                return s =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return LocLocalizer.Inst.SetupBuilderView.ErrorEmptyHostName;
                    return hostEntrys.Contains(s) || s == counterPart().Value ? LocLocalizer.Inst.SetupBuilderView.ErrorDuplicateHostName : null;
                };
            }

            HostName = RegisterProperty<string>(nameof(HostName))
                .WithValidator(SetError(HostNameValidator(() => SeedHostName)));

            SeedHostName = RegisterProperty<string>(nameof(SeedHostName))
                .WithValidator(SetError(HostNameValidator(() => HostName)));

            NewCommad
                .WithCanExecute(() => _buildRunning == 0 && HostName.IsValid && (!AddSeed.Value || SeedHostName.IsValid))
                .WithExecute(ExecuteBuild)
                .ThenRegister("CreateSeupCommand");
        }

        private void ExecuteBuild()
        {
            Interlocked.Increment(ref _buildRunning);
            CommandChanged();

            var stream = Context.System.EventStream;

            Task.Run(() =>
            {
                string hostName = HostName.Value;
                string seedHostName = SeedHostName.Value;

                UICall(TerminalLines.Clear);

                var builder = new SetupBuilder(hostName, AddSeed.Value ? seedHostName : null, _config, s => UICall(() => TerminalLines.Add(s)), stream);
                var id = Guid.NewGuid().ToString().Substring(0, 5);
                
                _server.AddPendingInstallations(id, builder.Run);
            }).ContinueWith(t =>
            {
                Interlocked.Decrement(ref _buildRunning);
                CommandChanged();
            });
        }

        private Func<TType, string?> SetError<TType>(Func<TType, string?> validator)
        {
            return data =>
            {
                var result = validator(data);
                if (!string.IsNullOrWhiteSpace(result))
                    CurrentError += result;

                return result;
            };
        }

        protected override void PreStart()
        {
            _subscribtion = _api.Event<HostEntryChanged>();
            base.PreStart();
        }

        protected override void PostStop()
        {
            _server.Dispose();
            _subscribtion?.Dispose();
            base.PostStop();
        }

        private int _buildRunning;

        public UICollectionProperty<string> TerminalLines { get; }

        public UIProperty<string> SeedHostName { get; private set; }

        public UIProperty<bool> AddSeed { get; }

        public UIProperty<string> CurrentError { get; private set; }

        public UIProperty<string> HostName { get; }
    }
}