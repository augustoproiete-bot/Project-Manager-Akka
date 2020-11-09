using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Tauron.Application.ServiceManager.Core.Managment.Data;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.ServiceManager.Core.Managment.States
{
    [State]
    public sealed class ServicesConfigState : StateBase<ServicesConfigurationData>
    {
        public static IValidator<string> MongoUrlValidator { get; } = new ValidateMongoUrl();

        public IEventSource<ConfigurationChangedEvent> ConfigChanged { get; }

        public ServicesConfigState(ExtendedMutatingEngine<MutatingContext<ServicesConfigurationData>> engine) 
            : base(engine)
        {
            ConfigChanged = engine.EventSource<ServicesConfigurationData, ConfigurationChangedEvent>();
        }

        private sealed class ValidateMongoUrl : AbstractValidator<MayMongoUrl>, IValidator<string>
        {
            public ValidateMongoUrl()
            {
                RuleFor(u => u.Url).Custom((s, context) =>
                {
                    try
                    {
                        var mongoUrl = MongoUrl.Create(s);
                        var client = new MongoClient(mongoUrl);
                        client.ListDatabases().MoveNext();
                        if (client.Cluster.Description.State != ClusterState.Connected)
                            context.AddFailure("Cluster Disconnected");
                    }
                    catch (Exception e)
                    {
                        context.AddFailure(e.Message);
                    }
                });
            }

            public ValidationResult Validate(string instance) => Validate(new MayMongoUrl(instance));

            public Task<ValidationResult> ValidateAsync(string instance, CancellationToken cancellation = new CancellationToken()) => ValidateAsync(new MayMongoUrl(instance), cancellation);
        }
    }
}