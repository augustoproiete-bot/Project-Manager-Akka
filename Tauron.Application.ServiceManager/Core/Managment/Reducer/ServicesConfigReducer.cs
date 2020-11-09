using System.Text;
using Akka.Configuration;
using FluentValidation;
using Serilog;
using Tauron.Application.ServiceManager.Core.Managment.Data;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.ServiceManager.Properties;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.ServiceManager.Core.Managment.Reducer
{
    [BelogsToState(typeof(ServicesConfigState))]
    public static class ServicesConfigReducer
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ServicesConfigReducer));

        [Validator]
        public static IValidator<ApplyMongoUrlAction> ApplyMongoUrlValidator { get; } = new InlineValidator<ApplyMongoUrlAction>
                                                                                        {
                                                                                            c => c.RuleFor(a => a.Url).SetValidator(ServicesConfigState.MongoUrlValidator)
                                                                                        };

        [Reducer]
        public static MutatingContext<ServicesConfigurationData> ApplyMongoUrl(MutatingContext<ServicesConfigurationData> state, ApplyMongoUrlAction action)
        {
            Log.Information("Update AppBase Configuration");

            const string snapshot = "akka.persistence.snapshot-store.plugin = \"akka.persistence.snapshot-store.mongodb\"";
            const string journal = "akka.persistence.journal.plugin = \"akka.persistence.journal.mongodb\"";

            const string connectionSnapshot = "akka.persistence.snapshot-store.mongodb.connection-string = \"{0}\"";
            const string connectionJournal = "akka.persistence.journal.mongodb.connection-string = \"{0}\"";

            var currentConfiguration = ConfigurationFactory.ParseString(state.Data.BaseConfiguration);

            var hasBase = currentConfiguration.HasPath("akka.persistence.journal.mongodb.connection-string ")
             || currentConfiguration.HasPath("akka.persistence.snapshot-store.mongodb.connection-string");

            if (!hasBase)
            {
                Log.Information("Apply Default Configuration");
                currentConfiguration = ConfigurationFactory.ParseString(Resources.BaseConfig).WithFallback(currentConfiguration);
            }

            var builder = new StringBuilder();

            builder
               .AppendLine(snapshot)
               .AppendLine(journal)
               .AppendFormat(connectionSnapshot, action.Url).AppendLine()
               .AppendFormat(connectionJournal, action.Url).AppendLine();

            currentConfiguration = ConfigurationFactory.ParseString(builder.ToString()).WithFallback(currentConfiguration);

            Log.Information("AppBase Configuration Updated");

            return state.WithChange(new ConfigurationChangedEvent(currentConfiguration.ToString(true)));
        }

    }
}