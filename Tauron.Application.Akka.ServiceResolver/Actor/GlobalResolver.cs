using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Tauron.Application.Akka.ServiceResolver.Core;
using Tauron.Application.Akka.ServiceResolver.Data;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Actor
{
    public class GlobalResolver : ResolveActorBase, IWithUnboundedStash
    {
        public sealed class Initialize
        {
            public ResolverSettings Settings { get; }

            public Initialize(ResolverSettings settings) 
                => Settings = settings;
        }

        public sealed class RegisterEndpoint
        {
            public IReadOnlyList<string> ProvidedServices { get; }

            public ServiceRequirement Requirement { get; }

            public IActorRef Host { get; }

            public RegisterEndpoint(IReadOnlyList<string> providedServices, ServiceRequirement requirement, IActorRef host)
            {
                ProvidedServices = providedServices;
                Requirement = requirement;
                Host = host;
            }
        }

        private sealed class ResolverService : ICanTell
        {
            private Task<IActorRef>? _resolver;

            private IActorRef ActorRef
            {
                get
                {
                    if(_resolver == null)
                        throw new InvalidOperationException("Resolver Task nicht erstellt");

                    if(!_resolver.IsCompleted)
                        _resolver.Wait();
                    
                    return _resolver.Result;
                }
            }

            public void Init(ActorSelection selection, ICanWatch watch)
            {
                _resolver = selection.ResolveOne(TimeSpan.FromMinutes(1)).ContinueWith(t =>
                                                                                       {
                                                                                           var r = t.Result;
                                                                                           watch.Watch(r);
                                                                                           return r;
                                                                                       });
            }

            public void Tell(object message, IActorRef sender) 
                => ActorRef.Tell(message, sender);
        }

        private sealed class ServiceEntry
        {
            public IReadOnlyList<string> Services { get; }

            public bool Supended { get; set; }

            public ServiceEntry(IReadOnlyList<string> services)
            {
                Services = services;
            }
        }

        private readonly Dictionary<string, ServiceEntry> _services = new Dictionary<string, ServiceEntry>();

        private readonly ResolverService _resolverService = new ResolverService();

        private ResolverSettings _resolverSettings = new ResolverSettings(Config.Empty);

        public IStash? Stash { get; set; }

        public GlobalResolver()
        {
            Receive<Initialize>(InitializeHandle);
            ReceiveAny(_ => Stash!.Stash());
        }

        private void InitializeHandle(Initialize obj)
        {
            if(obj.Settings.IsGlobal)
                Become(BecomeGlobalResolver);
            else
            {
                _resolverSettings = obj.Settings;
                if (!_resolverSettings.Verify(Context.System))
                    return;
                
                _resolverService.Init(Context.ActorSelection(obj.Settings.ResolverPath), Context);
                Become(BecomeGlobalProvider);
            }
            
            Stash!.UnstashAll();
        }

        #region Provider Implementation

        private void BecomeGlobalProvider()
        {
            Receive<RegisterEndpoint>(MakeEndpoint);
            Receive<QueryServiceRequest>(QueryServiceRequestProvider);
            Receive<Terminated>(Terminated);
        }

        private void Terminated(Terminated obj) 
            => Context.System.Terminate();

        private void QueryServiceRequestProvider(QueryServiceRequest obj) 
            => _resolverService.Tell(obj, Context.Sender);

        private void MakeEndpoint(RegisterEndpoint obj) 
            => _resolverService.Tell(new RegisterEndpointMessage(obj.Requirement, obj.ProvidedServices, _resolverSettings.Name), obj.Host);

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
            var key = _services.FirstOrDefault(e => e.Value.Services.Contains(obj.Name)).Key;

            var child = string.IsNullOrWhiteSpace(key) ? ActorRefs.Nobody : Context.Child(key);
            if(child.Equals(ActorRefs.Nobody))
                Context.Sender.Tell(new QueryServiceResponse(null));

            child.Forward(obj);
        }

        private void EndPointLost(EndPointManager.EndpointLostMessage obj)
        {
            _services.Remove(Context.Sender.Path.Name);
            TriggerChange();
        }

        private void SuspendedService(ToggleSuspendedMessage obj)
        {
            if(!_services.TryGetValue(Context.Sender.Path.Name, out var entry))
                return;

            entry.Supended = !entry.Supended;

            TriggerChange();
        }

        private void RegisterEndpointMessage(RegisterEndpointMessage obj)
        {
            if (_services.ContainsKey(obj.EndPointName))
            {
                Context.Sender.Tell(new RegistrationRejectedMessage());
                return;
            }

            _services[obj.EndPointName] = new ServiceEntry(obj.ProvidedServices);
            Context.ActorOf(Props.Create<EndPointManager>(Context.Sender, obj.Requirement), obj.EndPointName);

            TriggerChange();
        }

        private void TriggerChange()
        {
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