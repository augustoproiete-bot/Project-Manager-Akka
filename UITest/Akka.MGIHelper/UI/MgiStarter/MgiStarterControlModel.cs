using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;
using Akka.Actor;
using Akka.DI.Core;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.ProcessManager;
using Autofac;
using Tauron.Application.Wpf.Model;
using Tauron.Application.Wpf.ModelMessages;
using Tauron.Localization;

namespace Akka.MGIHelper.UI.MgiStarter
{
    public sealed class MgiStarterControlModel : UiActor
    {
        private readonly ProcessConfig _config;
        private readonly LocalHelper _localHelper;
        private readonly IActorRef _processManager;

        public MgiStarterControlModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, ProcessConfig config)
            : base(lifetimeScope, dispatcher)
        {
            Client = RegisterProperty<Process?>(nameof(Client)).OnChange(UpdateLabel);
            Kernel = RegisterProperty<Process?>(nameof(Kernel)).OnChange(UpdateLabel);
            Status = RegisterProperty<string?>(nameof(Status)).OnChange(UpdateLabel);
            InternalStart = RegisterProperty<bool>(nameof(InternalStart)).OnChange(UpdateLabel);
            StatusLabel = RegisterProperty<string?>(nameof(StatusLabel));

            _localHelper = new LocalHelper(Context);
            _config = config;
            _processManager = Context.ActorOf<ProcessManagerActor>("Process-Manager");
            var mgiStarting = Context.ActorOf(Context.DI().Props<MgiStartingActor>(), "Mgi-Starter");

            Receive<ProcessStateChange>(ProcessStateChangeHandler);
            Receive<MgiStartingActor.TryStartResponse>(TryStartResponseHandler);
            Receive<MgiStartingActor.StartStatusUpdate>(StatusUpdate);

            NewCommad
                .WithCanExecute(() => InternalStart == false)
                .WithExecute(() =>
                {
                    InternalStart += true;
                    mgiStarting.Tell(new MgiStartingActor.TryStart(_config, () =>
                    {
                        Client.Value?.Kill(true);
                        Kernel.Value?.Kill(true);
                    }));
                }).ThenRegister("TryStart");

            NewCommad
                .WithCanExecute(() => InternalStart == false && (Client != null || Kernel != null))
                .WithExecute(() =>
                {
                    Client.Value?.Kill(true);
                    Kernel.Value?.Kill(true);
                }).ThenRegister("TryStop");

            UpdateLabel();
        }

        private UIProperty<Process?> Client { get; set; }

        private UIProperty<Process?> Kernel { get; set; }

        private UIProperty<string?> Status { get; set; }

        private UIProperty<bool> InternalStart { get; set; }

        private UIProperty<string?> StatusLabel { get; set; }

        private void StatusUpdate(MgiStartingActor.StartStatusUpdate obj)
        {
            Status += obj.Status;
        }

        private void TryStartResponseHandler(MgiStartingActor.TryStartResponse obj)
        {
            InternalStart += false;
            CommandChanged();
        }

        private void ProcessStateChangeHandler(ProcessStateChange obj)
        {
            try
            {
                var processChange = obj.Change;
                var name = obj.Name;
                var process = obj.Process;
                switch (processChange)
                {
                    case ProcessChange.Started:
                        if (_config.Kernel.Contains(name))
                        {
                            ConfigProcess(process);
                            Kernel += process;
                        }

                        if (_config.Client.Contains(name))
                        {
                            ConfigProcess(process);
                            Client += process;
                        }

                        break;
                    case ProcessChange.Stopped:
                        if (_config.Kernel.Contains(name))
                            Kernel += null!;
                        if (_config.Client.Contains(name))
                            Client += null!;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (Kernel != null && Client != null)
                    Status += Context.Loc().RequestString("uistatusstartet");
                if (Kernel == null && Client == null)
                    Status += Context.Loc().RequestString("uistatusstopped");
            }
            catch (Exception e)
            {
                Status += "Fehler: " + e.Message;
            }
        }

        protected override void Initialize(InitEvent evt)
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            _processManager.Tell(new RegisterProcessList(Self, ImmutableArray<string>.Empty.Add(_config.Client).Add(_config.Kernel)));
        }

        private static void ConfigProcess(Process p)
        {
            if (p.PriorityClass != ProcessPriorityClass.High)
                p.PriorityClass = ProcessPriorityClass.High;
        }


        private void UpdateLabel()
        {
            var builder = new StringBuilder();

            var status = Status;
            var kernel = Kernel.Value != null;
            var client = Client.Value != null;
            if (!string.IsNullOrWhiteSpace(status) && status.Value?.StartsWith("Fehler:") == true)
            {
                StatusLabel = status;
                return;
            }

            builder.AppendLine(string.IsNullOrWhiteSpace(status) ? _localHelper.Unkowen : status);

            builder.Append("Kernel: ");
            builder.AppendLine(kernel ? _localHelper.GenericStart : _localHelper.GenericNotStart);
            builder.Append("Client: ");
            builder.AppendLine(client ? _localHelper.GenericStart : _localHelper.GenericNotStart);

            StatusLabel += builder.ToString();
        }

        private class LocalHelper
        {
            public LocalHelper(IActorContext context)
            {
                var loc = context.Loc();

                Unkowen = loc.RequestString("genericunkowen");
                GenericStart = loc.RequestString("genericstart");
                GenericNotStart = loc.RequestString("genericnotstart");
            }

            public string Unkowen { get; }

            public string GenericStart { get; }

            public string GenericNotStart { get; }
        }
    }
}