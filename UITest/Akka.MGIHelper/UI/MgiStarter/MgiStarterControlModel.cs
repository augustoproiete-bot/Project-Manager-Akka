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
        private class LocalHelper
        {
            public string Unkowen { get; }

            public string GenericStart { get; }

            public string GenericNotStart { get; }

            public LocalHelper(IActorContext context)
            {
                var loc = context.Loc();

                Unkowen = loc.RequestString("genericunkowen");
                GenericStart = loc.RequestString("genericstart");
                GenericNotStart = loc.RequestString("genericnotstart");
            }
        }

        private readonly ProcessConfig _config;
        private readonly IActorRef _processManager;
        private readonly LocalHelper _localHelper;

        private Process? Client
        {
            get => Get<Process>();
            set => Set(value, UpdateLabel);
        }

        private Process? Kernel
        {
            get => Get<Process>();
            set => Set(value, UpdateLabel);
        }

        private string? Status
        {
            get => Get<string>();
            set => Set(value, UpdateLabel);
        }

        private bool? InternalStart
        {
            get => Get<bool>();
            set => Set(value, UpdateLabel);
        }

        private string? StatusLabel
        {
            set => Set(value);
        }

        public MgiStarterControlModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, ProcessConfig config) 
            : base(lifetimeScope, dispatcher)
        {
            _localHelper = new LocalHelper(Context);
            _config = config;
            _processManager = Context.ActorOf<ProcessManagerActor>("Process-Manager");
            var mgiStarting = Context.ActorOf(Context.DI().Props<MgiStartingActor>(), "Mgi-Starter");

            Receive<ProcessStateChange>(ProcessStateChangeHandler);
            Receive<MgiStartingActor.TryStartResponse>(TryStartResponseHandler);
            Receive<MgiStartingActor.StartStatusUpdate>(StatusUpdate);

            RegisterCommand("TryStart", o =>
                                        {
                                            InternalStart = true;
                                            mgiStarting.Tell(new MgiStartingActor.TryStart(_config, () =>
                                                                                                     {
                                                                                                         Client?.Kill(true);
                                                                                                         Kernel?.Kill(true);
                                                                                                     }));
                                        }, o => InternalStart == false);

            RegisterCommand("TryStop", o =>
                                       {
                                           Client?.Kill(true);
                                           Kernel?.Kill(true);
                                       }, o => InternalStart == false && (Client != null || Kernel != null));

            UpdateLabel();
        }

        private void StatusUpdate(MgiStartingActor.StartStatusUpdate obj) 
            => Status = obj.Status;

        private void TryStartResponseHandler(MgiStartingActor.TryStartResponse obj) 
            => InternalStart = false;

        private void ProcessStateChangeHandler(ProcessStateChange obj)
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
                        Kernel = process;
                    }
                    if (_config.Client.Contains(name))
                    {
                        ConfigProcess(process);
                        Client = process;
                    }
                    break;
                case ProcessChange.Stopped:
                    if (name == _config.Kernel)
                        Kernel = null;
                    if (name == _config.Client)
                        Client = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(Kernel != null && Client != null)
                Status = Context.Loc().RequestString("uistatusstartet");
            if (Kernel == null && Client == null)
                Status = Context.Loc().RequestString("uistatusstopped");
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
            var kernel = Kernel != null;
            var client = Client != null;
            builder.AppendLine(string.IsNullOrWhiteSpace(status) ? _localHelper.Unkowen : status);

            builder.Append("Kernel: ");
            builder.AppendLine(kernel ? _localHelper.GenericStart : _localHelper.GenericNotStart);
            builder.Append("Client: ");
            builder.AppendLine(client ? _localHelper.GenericStart : _localHelper.GenericNotStart);

            StatusLabel = builder.ToString();
        }
    }
}