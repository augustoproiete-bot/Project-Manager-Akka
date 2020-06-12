using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Configuration;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Data;
using Tauron.Application.Akka.ServiceResolver.Messages;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public class GlobalResolver : ReceiveActor, IWithUnboundedStash
    {
        private readonly ILoggingAdapter _log;
        private readonly ResolverService _resolverService = new ResolverService();
        private readonly Dictionary<string, ServiceEntry> _services = new Dictionary<string, ServiceEntry>();
        private ResolverSettings _resolverSettings = new ResolverSettings(Config.Empty);

        public GlobalResolver()
        {
            _log = Context.GetLogger();
            Receive<Initialize>(InitializeHandle);

            ReceiveAny(_ => Stash!.Stash());
        }

        public IStash? Stash { get; set; }

        private void InitializeHandle(Initialize obj)
        {
            if (obj.Settings.IsGlobal)
            {
                _log.Info("Initialize as Global Resolver");
                Become(BecomeGlobalResolver);
            }
            else
            {
                _log.Info("Initialize as Local Resolver");
                _resolverSettings = obj.Settings;

                _resolverService.Init(DistributedPubSub.Get(Context.System), _log);
                Become(BecomeGlobalProvider);

            }

            Stash!.UnstashAll();
        }

        public sealed class Initialize
        {
            public Initialize(ResolverSettings settings) => Settings = settings;

            public ResolverSettings Settings { get; }
        }

        public sealed class RegisterEndpoint
        {
            public RegisterEndpoint(IReadOnlyList<string> providedServices, ServiceRequirement requirement, IActorRef host)
            {
                ProvidedServices = providedServices;
                Requirement = requirement;
                Host = host;
            }

            public IReadOnlyList<string> ProvidedServices { get; }

            public ServiceRequirement Requirement { get; }

            public IActorRef Host { get; }
        }

        private sealed class ResolverService
        {
            private IActorRef _mediator = ActorRefs.Nobody;

            public void Tell(object message) 
                => _mediator.Tell(new Publish(Topics.ServiceResolver, message));

            public void Init(DistributedPubSub pubsub, ILoggingAdapter log)
            {
                log.Info("Try Resolve GlobalResolver");

                _mediator = pubsub.Mediator;
            }
        }

        private sealed class ServiceEntry
        {
            public ServiceEntry(IReadOnlyList<string> services) 
                => Services = services;

            public IReadOnlyList<string> Services { get; }

            public bool Supended { get; set; }
        }

        #region Provider Implementation

        private void BecomeGlobalProvider()
        {
            var pubsub = DistributedPubSub.Get(Context.System);
            pubsub.Mediator.Tell(new Subscribe(Topics.ServiceResolver, Self));

            Receive<SubscribeAck>(sa => { });

            Receive<RegisterEndpoint>(MakeEndpoint);
            Receive<QueryServiceRequest>(QueryServiceRequestProvider);
            Receive<Terminated>(Terminated);
        }

        private void Terminated(Terminated obj)
        {
            _log.Error("Conection to Global Resolver Lost. Terminate System. {Path}", obj.ActorRef.Path);
            Context.System.Terminate();
        }

        private void QueryServiceRequestProvider(QueryServiceRequest obj) 
            => _resolverService.Tell(obj.WithSender(Context.Sender));

        private void MakeEndpoint(RegisterEndpoint obj) 
            => _resolverService.Tell(new RegisterEndpointMessage(obj.Requirement, obj.ProvidedServices, _resolverSettings.Name).WithHost(obj.Host));

        #endregion

        #region Resolver Implementation

        private void BecomeGlobalResolver()
        {
            var pubsub = DistributedPubSub.Get(Context.System);
            pubsub.Mediator.Tell(new Subscribe(Topics.ServiceResolver, Self));

            Receive<SubscribeAck>(sa => {});

            Receive<RegisterEndpointMessage>(RegisterEndpointMessage);
            Receive<EndPointManager.EndpointLostMessage>(EndPointLost);
            Receive<ToggleSuspendedMessage>(SuspendedService);
            Receive<QueryServiceRequest>(QueryServiceRequest);
        }

        private void QueryServiceRequest(QueryServiceRequest obj)
        {
            _log.Info("Query Service Request income");
            var key = _services.FirstOrDefault(e => e.Value.Services.Contains(obj.Name)).Key;

            var child = string.IsNullOrWhiteSpace(key) ? ActorRefs.Nobody : Context.Child(key);
            if (child.Equals(ActorRefs.Nobody))
            {
                _log.Warning("No Service Found {Name}--{EndPoint}", obj.Name, key);
                obj.Sender.Tell(new QueryServiceResponse(null));
            }

            child.Forward(obj);
        }

        private void EndPointLost(EndPointManager.EndpointLostMessage obj)
        {
            _log.Warning("Endpoint Connection Lost. Removing Services");
            _services.Remove(Context.Sender.Path.Name);
            TriggerChange();
        }

        private void SuspendedService(ToggleSuspendedMessage obj)
        {
            if (!_services.TryGetValue(Context.Sender.Path.Name, out var entry))
                return;

            entry.Supended = !entry.Supended;

            TriggerChange();
        }

        private void RegisterEndpointMessage(RegisterEndpointMessage obj)
        {
            _log.Info("Try Register Endpoint {Endpoint}", obj.EndPointName);
            if (_services.ContainsKey(obj.EndPointName))
            {
                _log.Info("Endpoint Already Registrated {Endpoint", obj.EndPointName);
                Context.Sender.Tell(new RegistrationRejectedMessage());
                return;
            }

            _log.Info("Create Endpoint Manager");
            _services[obj.EndPointName] = new ServiceEntry(obj.ProvidedServices);
            Context.ActorOf(Props.Create<EndPointManager>(Context.Sender, obj.Requirement), obj.EndPointName);

            TriggerChange();
        }

        private void TriggerChange()
        {
            _log.Info("Trigger Recheck of Sercive Requirement");
            foreach (var actorRef in Context.GetChildren())
            {
                actorRef.Tell(new EndPointManager.ServiceChangeMessages(
                    _services
                       .Select(p => p.Value)
                       .Where(s => !s.Supended)
                       .SelectMany(s => s.Services).ToList().AsReadOnly()));
            }
        }

        #endregion
    }
}