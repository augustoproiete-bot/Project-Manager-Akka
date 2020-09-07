using System;
using System.Text;
using System.Windows.Threading;
using Akka;
using Akka.Configuration;
using Akka.Event;
using Autofac;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Properties;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ConfigurationViewModel : UiActor
    {
        

        public ConfigurationViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, AppConfig appConfig) 
            : base(lifetimeScope, dispatcher)
        {
            void ConfigurationChanged(string text)
            {
                if(text == appConfig.CurrentConfig) return;
                
                try
                {
                    Log.Info("Update Configuration");
                    ConfigurationFactory.ParseString(text);
                    appConfig.CurrentConfig = text;
                    ErrorText += string.Empty;
                }
                catch(Exception e)
                {
                    Log.Info(e, "Configuration Update Invalid");
                    ErrorText += e.Message;
                }
            }

            string? ValidateMongoConnection(string url)
            {
                try
                {
                    Log.Info("Check Mongo Url {URL}", url);

                    var mongoUrl = MongoUrl.Create(url);
                    var client = new MongoClient(mongoUrl);
                    client.ListDatabases().MoveNext();
                    return client.Cluster.Description.State == ClusterState.Connected ? null : "Cluster Disconnected";
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Mongo URL Invalid {URL}", url);

                    return e.Message;
                }
            }

            void ApplyConfiguration(string connectionUrl)
            {
                Log.Info("Update AppBase Configuration");

                const string snapshot = "akka.persistence.snapshot-store.plugin = \"akka.persistence.snapshot-store.mongodb\"";
                const string journal = "akka.persistence.journal.plugin = \"akka.persistence.journal.mongodb\"";
                
                const string connectionSnapshot = "akka.persistence.snapshot-stor.mongodb.connection-string = \"{0}\"";
                const string connectionJournal = "akka.persistence.journal.mongodb.connection-string = \"{0}\"";

                var currentConfiguration = ConfigurationFactory.ParseString(appConfig.CurrentConfig);

                bool hasBase = currentConfiguration.HasPath("akka.persistence.journal.mongodb.connection-string ")
                                || currentConfiguration.HasPath("akka.persistence.snapshot-store.mongodb.connection-string");

                if (!hasBase)
                {
                    Log.Info("Apply Default Configuration");
                    currentConfiguration = ConfigurationFactory.ParseString(Resources.BaseConfig).WithFallback(currentConfiguration);
                }

                var builder = new StringBuilder();

                builder
                    .AppendLine(snapshot)
                    .AppendLine(journal)
                    .AppendFormat(connectionSnapshot, connectionUrl).AppendLine()
                    .AppendFormat(connectionJournal, connectionUrl).AppendLine();

                currentConfiguration = ConfigurationFactory.ParseString(builder.ToString()).WithFallback(currentConfiguration);

                ConfigText += currentConfiguration.ToString(true);
                Log.Info("AppBase Configuration Updated");
            }

            ErrorText = RegisterProperty<string>(nameof(ErrorText));

            ConfigText = RegisterProperty<string>(nameof(ConfigText))
                .OnChange(ConfigurationChanged)
                .WithDefaultValue(appConfig.CurrentConfig)
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

            NewCommad
                .WithCanExecute(() => ConnectionString.IsValid)
                .WithExecute(() => ErrorText += ValidateMongoConnection(ConnectionString) ?? string.Empty)
                .ThenRegister("ValidateConnection");

            NewCommad
                .WithCanExecute(() => ConnectionString.IsValid)
                .WithExecute(() =>
                {
                    try
                    {
                        Log.Info("Apply Connection");
                        ValidateMongoConnection(ConnectionString);
                        ApplyConfiguration(ConnectionString);
                        Log.Info("Apply Connection Compled");
                    }
                    catch (Exception e)
                    {
                        Log.Info(e, "Apply Connection Failed");
                        ErrorText += e.Message;
                    }
                })
                .ThenRegister("ApplyConnection");
        }

        public UIProperty<string> ConnectionString { get; }

        public UIProperty<bool> IsSetupVisible { get; set; }

        public UIProperty<string> ErrorText { get; set; }

        public UIProperty<string> ConfigText { get; set; }

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