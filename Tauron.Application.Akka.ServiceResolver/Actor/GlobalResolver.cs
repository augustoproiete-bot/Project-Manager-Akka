using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Data;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public class GlobalResolver : ResolveActorBase, IWithUnboundedStash
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
                if (!_resolverSettings.Verify(Context.System))
                {
                    _log.Error("Resolver Settings Invalid Become Faulted");
                    Become(Faulted);
                }
                else
                {
                    _resolverService.Init(Context.ActorSelection(obj.Settings.ResolverPath), Context, _log);
                    Become(BecomeGlobalProvider);
                }
            }

            Stash!.UnstashAll();
        }

        private void Faulted()
        {
            ReceiveAny(_ => throw new InvalidResolverStateExcepion("Setting Verification Failed"));
        }

        public sealed class Initialize
        {
            public Initialize(ResolverSettings settings)
            {
                Settings = settings;
            }

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

        private sealed class ResolverService : ICanTell
        {
            private Task<IActorRef>? _resolver;

            private IActorRef ActorRef
            {
                get
                {
                    if (_resolver == null)
                        throw new InvalidOperationException("Resolver Task nicht erstellt");

                    if (!_resolver.IsCompleted)
                        _resolver.Wait();

                    return _resolver.Result;
                }
            }

            public void Tell(object message, IActorRef sender)
            {
                ActorRef.Tell(message, sender);
            }

            public void Init(ActorSelection selection, ICanWatch watch, ILoggingAdapter log)
            {
                log.Info("Try Resolve GlobalResolver");

                _resolver = selection.ResolveOne(TimeSpan.FromMinutes(1)).ContinueWith(t =>
                {
                    var r = t.Result;
                    log.Info("Global Resolver Found {Path}", r.Path);
                    watch.Watch(r);
                    return r;
                });
            }
        }

        private sealed class ServiceEntry
        {
            public ServiceEntry(IReadOnlyList<string> services)
            {
                Services = services;
            }

            public IReadOnlyList<string> Services { get; }

            public bool Supended { get; set; }
        }

        #region Provider Implementation

        private void BecomeGlobalProvider()
        {
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
        {
            _resolverService.Tell(obj, Context.Sender);
        }

        private void MakeEndpoint(RegisterEndpoint obj)
        {
            _resolverService.Tell(new RegisterEndpointMessage(obj.Requirement, obj.ProvidedServices, _resolverSettings.Name), obj.Host);
        }

        #endregion

        #region Resolver Implementation

        private void BecomeGlobalResolver()
        {
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
                Context.Sender.Tell(new QueryServiceResponse(null));
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
                actorRef.Tell(new EndPointManager.ServiceChangeMessages(
                    _services
                        .Select(p => p.Value)
                        .Where(s => !s.Supended)
                        .SelectMany(s => s.Services).ToList().AsReadOnly()));
        }

        #endregion
    }
}