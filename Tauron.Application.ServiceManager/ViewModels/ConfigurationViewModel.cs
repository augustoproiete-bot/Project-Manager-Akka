using System;
using System.Linq;
using System.Windows.Threading;
using Akka.Configuration;
using Akka.Event;
using Autofac;
using MongoDB.Driver;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Wpf.Model;
using Tauron.Operations;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : StateUIActor
    {
        public ConfigurationViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker actionInvoker) 
            : base(lifetimeScope, dispatcher, actionInvoker)
        {
            ErrorText = RegisterProperty<string>(nameof(ErrorText));

            ConfigText = RegisterProperty<string>(nameof(ConfigText))
                .WithValidator(s =>
                {
                    try
                    {
                        ConfigurationFactory.ParseString(s);
                        return null;
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                });

            GetState<ServicesConfigState>().Query(EmptyQuery.Instance)
               .ContinueWith(c => ConfigText.Set(c.Result?.BaseConfiguration));

            IsSetupVisible = RegisterProperty<bool>(nameof(IsSetupVisible));

            Receive<StartConfigurationSetup>(_ => IsSetupVisible += true);

            NewCommad
                .WithExecute(() =>
                {
                    IsSetupVisible += false;
                    Context.System.EventStream.Publish(StartInitialHostSetup.Get);
                })
                .ThenRegister("SetupNext");

            ConnectionString = RegisterProperty<string>(nameof(ConnectionString))
                .WithValidator(s =>
                {
                    try
                    {
                        MongoUrl.Create(s);
                        return null;
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                });

            void ValidateMongoUrl()
            {
                var result = ServicesConfigState.MongoUrlValidator.Validate(ConnectionString.Value);
                if (result.IsValid)
                    ErrorText += string.Empty;
                else
                    ErrorText += result.Errors.FirstOrDefault()?.ErrorMessage ?? string.Empty;
            }

            NewCommad
               .WithCanExecute(b => b.FromProperty(ConnectionString.IsValid))
               .WithExecute(ValidateMongoUrl)
               .ThenRegister("ValidateConnection");

            NewCommad
               .WithCanExecute(b => b.FromProperty(ConnectionString.IsValid))
               .ToStateAction(() => new ApplyMongoUrlAction(ConnectionString.Value))
               .ThenRegister("ApplyConnection");
        }

        public UIProperty<string> ConnectionString { get; }

        public UIProperty<bool> IsSetupVisible { get; set; }

        public UIProperty<string> ErrorText { get; set; }

        public UIProperty<string> ConfigText { get; set; }

        protected override void OnOperationCompled(IOperationResult result)
        {
            base.OnOperationCompled(result);

            if (result.Ok)
                ErrorText += string.Empty;
            else if (string.IsNullOrWhiteSpace(result.Error))
                ErrorText += result.Error ?? string.Empty;
        }

        protected override void PostStop()
        {
            Context.System.EventStream.Unsubscribe<StartConfigurationSetup>(Self);
            base.PostStop();
        }

        protected override void PreStart()
        {
            Context.System.EventStream.Subscribe<StartConfigurationSetup>(Self);
            base.PreStart();
        }
    }
}