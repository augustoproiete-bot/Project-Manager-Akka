using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Cluster;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.ViewModels
{
    public sealed class ActualNode : ObservableObject
    {
        private string? _url;
        private string? _name;
        private MemberStatus _memberStatus;
        private MemberAddress? _uniqueAddress;
        private LocLocalizer? _localizer;
        public string? Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string? Url
        {
            get => _url;
            set
            {
                if (value == _url) return;
                _url = value;
                OnPropertyChanged();
            }
        }

        public MemberStatus MemberStatus
        {
            get => _memberStatus;
            set
            {
                if (value == _memberStatus) return;
                _memberStatus = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Roles { get; } = new UIObservableCollection<string>();

        public void UpdateModel(ActorSystem system, Member member, LocLocalizer localizer)
        {
            _uniqueAddress = MemberAddress.From(member.UniqueAddress);
            _localizer = localizer;

            Roles.Clear();
            foreach (var role in member.Roles)
                Roles.Add(role);

            Url = $"{member.UniqueAddress.Address} - {member.UniqueAddress.Uid}";
            MemberStatus = member.Status;

            Name =localizer.Common.Querying;

            Task.Delay(TimeSpan.FromSeconds(5))
                .ContinueWith(t =>
                {
                    ServiceRegistry.GetRegistry(system)
                        .QueryService()
                        .ContinueWith(UpdateName);
                });
        }

        public bool IsSame(Member mem)
            => _uniqueAddress?.Equals(mem.UniqueAddress) ?? false;

        public static ActualNode New(ActorSystem system, Member mem, LocLocalizer localizer)
        {
            var node = new ActualNode();
            node.UpdateModel(system, mem, localizer);
            return node;
        }

        private void UpdateName(Task<QueryRegistratedServicesResponse> taskRsponse)
        {
            try
            {
                var response = taskRsponse.Result;
                if (_uniqueAddress == null) return;

                var ent = response.Services.FirstOrDefault(ms => ms.Address == _uniqueAddress);
                if (ent != null)
                    Name = ent.Name;
            }
            catch (Exception e)
            {
                Name = $"{_localizer?.Common.Error} -- {e.Message}";
            }
        }
    }

    [UsedImplicitly]
    public sealed class NodeViewModel : UiActor
    {
        private readonly LocLocalizer _localizer;

        public NodeViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer) 
            : base(lifetimeScope, dispatcher)
        {
            _localizer = localizer;
            Nodes = this.RegisterUiCollection<ActualNode>(nameof(Nodes)).AndAsync();
            CurrentStatus = RegisterProperty<string>(nameof(CurrentStatus)).WithDefaultValue(localizer.MemberStatus.SelfOffline);

            #region MemberEvents

            void UpdateNode(Member mem)
            {
                var node = Nodes.FirstOrDefault(an => an.IsSame(mem));
                if (node == null)
                {
                    Log.Info("Add Node {Member}", mem);
                    Nodes.Add(ActualNode.New(Context.System, mem, LocLocalizer.Inst));
                }
                else if (mem.Status == MemberStatus.Removed)
                {
                    Log.Info("Remove Node {Member}", mem);
                    Nodes.Remove(node);
                }
                else
                {
                    Log.Info("Update Node {Member}", mem);
                    node.UpdateModel(Context.System, mem, LocLocalizer.Inst);
                }
            }

            Receive<ClusterEvent.IMemberEvent>(mem => UpdateNode(mem.Member));

            #endregion
        }

        protected override void PreStart()
        {
            base.PreStart();
            var cluster = Cluster.Get(Context.System);
            
            cluster.RegisterOnMemberUp(() => CurrentStatus += _localizer.MemberStatus.SelfOnline);
            cluster.Subscribe(Context.Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, typeof(ClusterEvent.IMemberEvent));
        }

        public UIProperty<string> CurrentStatus { get; set; }

        public UICollectionProperty<ActualNode> Nodes { get; }
    }
}