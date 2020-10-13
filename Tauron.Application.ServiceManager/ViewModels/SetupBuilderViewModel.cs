using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands.Administration.Host;
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
            _server = new SetupServer(s => UICall(() => TerminalLines!.Add(s)), Context.System.Settings.Config);

            AddShortcut = RegisterProperty<bool>(nameof(AddShortcut));
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
                .WithValidator(SetError(HostNameValidator(() => SeedHostName!)))
                .OnChange(s => SeedHostName += s + "_Seed");

            SeedHostName = RegisterProperty<string>(nameof(SeedHostName))
                .WithValidator(SetError(HostNameValidator(() => HostName)));

            NewCommad
               .WithCanExecute(b =>
                    b.And(
                        b.FromProperty(_buildRunning, i => i == 0),
                        b.FromProperty(HostName.IsValid),
                        b.Or(
                            b.FromProperty(AddSeed, s => !s),
                            b.FromProperty(SeedHostName.IsValid))))
               .WithExecute(ExecuteBuild)
               .ThenRegister("CreateSeupCommand");
        }

        private void ExecuteBuild()
        {
            _buildRunning.Value++;

            var stream = Context.System.EventStream;

            string hostName = HostName.Value;
            string seedHostName = SeedHostName.Value;

            UICall(TerminalLines.Clear);

            var builder = new SetupBuilder(hostName, AddSeed.Value ? seedHostName : null, _config, s => UICall(() => TerminalLines.Add(s)), stream);
            var id = Guid.NewGuid().ToString().Substring(0, 5);

            _server.AddPendingInstallations(id, builder.Run, AddShortcut);

            AddShortcut.Set(false);
            AddSeed.Set(false);
            HostName.Set(string.Empty);
            SeedHostName.Set(string.Empty);

            _buildRunning.Value--;
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

        private readonly QueryProperty<int> _buildRunning = QueryProperty.Create(0);

        public UICollectionProperty<string> TerminalLines { get; }

        public UIProperty<string> SeedHostName { get; private set; }

        public UIProperty<bool> AddSeed { get; }

        public UIProperty<string> CurrentError { get; private set; }

        public UIProperty<string> HostName { get; }

        public UIProperty<bool> AddShortcut { get; }
    }
}