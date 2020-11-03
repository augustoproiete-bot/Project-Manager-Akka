using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using CacheManager.Core;
using FluentValidation;
using Tauron;
using Tauron.Akka;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Operations;

namespace AkkaTest
{
    internal static class Program
    {
        public sealed class UserSourceFactory : IDataSourceFactory
        {
            private sealed class UserDataSource : IQueryableDataSource<UserData>
            {
                private static readonly UserData Empty = new UserData(string.Empty, DateTime.MinValue, DateTime.MinValue, false, false, string.Empty, true);

                private readonly ConcurrentDictionary<string, UserData> _user;
                private UserData? _next;

                public UserDataSource(ConcurrentDictionary<string, UserData> user) => _user = user;

                public UserData GetData(IQuery query)
                {
                    switch (query)
                    {
                        case UserQuery q:
                            if (_user.TryGetValue(q.Name, out _next))
                                return _next;
                            _next = _user.Values.FirstOrDefault(u => u.Name == q.Name);
                            break;
                        case StringQuery info:
                            _next = _user.GetOrAdd(info.Name, s => new UserData(s, DateTime.Now, DateTime.Now, true, true, s, false));
                            break;
                        default:
                            _next = Empty;
                            break;
                    }

                    return _next ?? Empty;
                }

                public void SetData(IQuery query, UserData data)
                {
                    if (data.IsDeleted)
                        _user.TryRemove(data.Id, out _);
                    else
                        _user.AddOrUpdate(data.Id, data, (s, userData) => data.SetUnchanged());
                }

            }

            private sealed class UsersDataSource : IQueryableDataSource<UserList>
            {
                private readonly ConcurrentDictionary<string, UserData> _userDatas;

                public UsersDataSource(ConcurrentDictionary<string, UserData> userDatas) => _userDatas = userDatas;

                public UserList GetData(IQuery query) => new UserList(_userDatas.Values);

                public void SetData(IQuery query, UserList data)
                {
                }
            }

            private readonly ConcurrentDictionary<string, UserData> _userDatas = new ConcurrentDictionary<string, UserData>();

            public Func<IQueryableDataSource<TData>> Create<TData>()
                where TData : class, IStateEntity
            {
                var type = typeof(TData);

                if (type == typeof(UserData))
                    return () => (new UserDataSource(_userDatas)).As<IQueryableDataSource<TData>>()!;
                if (type == typeof(UserList))
                    return () => (new UsersDataSource(_userDatas)).As<IQueryableDataSource<TData>>()!;

                throw new InvalidOperationException("No Data Source Found");
            }
        }

        public sealed class UserList : INoCache, ICollection<UserData>, IStateEntity
        {
            private readonly ICollection<UserData> _users;

            public UserList(ICollection<UserData> users) => _users = users;
            public IEnumerator<UserData> GetEnumerator()
            {
                return _users.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _users).GetEnumerator();
            }

            void ICollection<UserData>.Add(UserData item)
            {
                _users.Add(item);
            }

            void ICollection<UserData>.Clear()
            {
                _users.Clear();
            }

            public bool Contains(UserData item)
            {
                return _users.Contains(item);
            }

            public void CopyTo(UserData[] array, int arrayIndex)
            {
                _users.CopyTo(array, arrayIndex);
            }

            bool ICollection<UserData>.Remove(UserData item)
            {
                return _users.Remove(item);
            }

            public int Count => _users.Count;

            public bool IsReadOnly => _users.IsReadOnly;
            bool IStateEntity.IsDeleted => false;
            string IStateEntity.Id => "List";
        }

        public sealed class UserData : IChangeTrackable, IStateEntity, ICanApplyChange<UserData>
        {
            public string Id { get; }

            public string Name { get; }

            public DateTime LastAccess { get; }

            public DateTime CreationTime { get; }

            public bool IsChanged { get; }

            public bool IsNew { get; }

            public bool IsDeleted { get; }

            public UserData(string name, DateTime lastAccess, DateTime creationTime, bool isChanged, bool isNew, string id, bool isDeleted)
            {
                Name = name;
                LastAccess = lastAccess;
                CreationTime = creationTime;
                IsChanged = isChanged;
                IsNew = isNew;
                Id = id;
                IsDeleted = isDeleted;
            }

            private UserData CreateFromNew() => !IsNew ? this : new UserData(Name, LastAccess, CreationTime, true, false, Name, false);

            private UserData UpdateName(string name)
                => new UserData(name, DateTime.Now, CreationTime, true, false, Id, false);

            public UserData SetUnchanged()
                => new UserData(Name, LastAccess, CreationTime, false, false, Id, false);

            private UserData MarkDelete()
                => new UserData(Name, DateTime.Now, CreationTime, true, false, Id, true);

            public UserData Apply(MutatingChange apply)
            {
                return apply switch
                {
                    RenameChange rename => UpdateName(rename.NewName),
                    CreateUserChange _ => CreateFromNew(),
                    DeleteUserChange change => MarkDelete(),
                    _ => this
                };
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Id: {Id}");
                builder.AppendLine($"Name: {Name}");
                builder.AppendLine($"Letzter Zugriff: {LastAccess:f}");
                builder.AppendLine($"Erstell Datum: {CreationTime:f}");

                return builder.ToString();
            }
        }

        public sealed class StringQuery : IQuery
        {
            public string Name { get; }

            public StringQuery(string name) => Name = name;

            public string ToHash() => Name;
        }

        public sealed class EmptyQuery : IQuery, INoCache
        {
            public string ToHash() => "Empty";
        }

        public sealed class KillQuery : IQuery, INoCache
        {
            public string ToHash() => "Kill";
        }

        public sealed class UserQuery : IQuery, INoCache
        {
            public string Name { get; }

            public UserQuery(string name) => Name = name;

            public string ToHash() => Name;
        }

        public sealed class CreateUserAction : IStateAction
        {
            public string ActionName => "CreateUser";

            public IQuery Query { get; }

            public CreateUserAction(string userId) => Query = new StringQuery(userId);
        }

        public sealed class RenameUserAction : IStateAction
        {
            public string ActionName => "RenameUser";

            public IQuery Query { get; }

            public string NewName { get; }

            public RenameUserAction(string query, string newName)
            {
                Query = new StringQuery(query);
                NewName = newName;
            }
        }

        public sealed class DeleteUserAction : IStateAction
        {
            public string ActionName => "DeleteUser";
            public IQuery Query { get; }

            public DeleteUserAction(string name) => Query = new UserQuery(name);
        }

        public sealed class QueryUserAction : IStateAction
        {
            public string ActionName => "QueryUserName";
            public IQuery Query { get; }

            public QueryUserAction(string query) => Query =new UserQuery(query);
        }

        public sealed class KillApplicationAction : IStateAction
        {
            public string ActionName => "KillApp";
            public IQuery Query { get; }

            public KillApplicationAction() => Query = new KillQuery();
        }

        public sealed class QueryUsersAction : IStateAction
        {
            public string ActionName => "QueryUsers";
            public IQuery Query { get; } = new EmptyQuery();
        }

        public sealed class QueryUsersChange : MutatingChange
        {
            public UserList Users { get; }

            public QueryUsersChange(UserList users) => Users = users;
        }

        public sealed class KillChange : MutatingChange
        {
        }

        public sealed class QueryUserChange : MutatingChange
        {
            public UserData Data { get; }

            public QueryUserChange(UserData data) => Data = data;
        }

        public sealed class RenameChange : MutatingChange
        {
            public string NewName { get; }

            public string Id { get; }

            public RenameChange(string newName, string id)
            {
                NewName = newName;
                Id = id;
            }
        }

        public sealed class CreateUserChange : MutatingChange
        {
            public string Id { get; }

            public CreateUserChange(string id)
            {
                Id = id;
            }
        }

        public sealed class DeleteUserChange : MutatingChange
        {
            public UserData Data { get; }

            public DeleteUserChange(UserData data) => Data = data;
        }

        [State]
        public sealed class KillState : StateBase<UserData>
        {
            public KillState(QueryableMutatingEngine<MutatingContext<UserData>> engine, ActorSystem system)
                : base(engine)
            {
                engine.EventSource<UserData, KillChange>().RespondOn(c => system.Terminate());
            }

            //public IEventSource<KillChange> Kill { get; }
        }

        [State]
        public sealed class UserStade : StateBase<UserData>
        {
            [Cache(UseParent = true)]
            public static void ConfigurateCache(ConfigurationBuilderCachePart config) => config.WithDictionaryHandle();

            public UserStade(QueryableMutatingEngine<MutatingContext<UserData>> engine)
                : base(engine)
            {
                QueryUserName = engine.EventSource<UserData, QueryUserChange>();
                UserRenamed = engine.EventSource<UserData, RenameChange>();
                UserCreated = engine.EventSource<UserData, CreateUserChange>();
            }

            public IEventSource<QueryUserChange> QueryUserName { get; }

            public IEventSource<RenameChange> UserRenamed { get; }

            public IEventSource<CreateUserChange> UserCreated { get; }
        }

        [State]
        public sealed class UsersStade : StateBase<UserList>
        {
            public UsersStade(QueryableMutatingEngine<MutatingContext<UserList>> engine) 
                : base(engine)
                => QueryUsers = engine.EventSource<UserList, QueryUsersChange>();

            public IEventSource<QueryUsersChange> QueryUsers { get; }
        }

        [BelogsToState(typeof(KillState))]
        public static class KillStateReducer
        {
            [Reducer]
            public static MutatingContext<UserData> Kill(MutatingContext<UserData> state, KillApplicationAction action)
                => state.WithChange(new KillChange());
        }

        [BelogsToState(typeof(UserStade))]
        public static class UserStateReducer
        {
            [Reducer]
            public static MutatingContext<UserData> CreateUser(MutatingContext<UserData> state, CreateUserAction action)
                => state.WithChange(new CreateUserChange(state.Data.Name));

            [Validator]
            public static RenameValidator RenameActionValidator => new RenameValidator();

            [Reducer]
            public static MutatingContext<UserData> RenameUser(MutatingContext<UserData> state, RenameUserAction action)
                => state.WithChange(new RenameChange(action.NewName, state.Data.Id));
            
            [Reducer]
            public static MutatingContext<UserData> DeleteUser(MutatingContext<UserData> state, DeleteUserAction action)
                => state.WithChange(new DeleteUserChange(state.Data));

            public sealed class RenameValidator : AbstractValidator<RenameUserAction>
            {
                public RenameValidator() => RuleFor(a => a.NewName).NotEmpty();
            }
        }

        [BelogsToState(typeof(UserStade))]
        public static class UserStateQuerys
        {
            [Reducer]
            public static MutatingContext<UserData> QueryUserName(MutatingContext<UserData> state, QueryUserAction action)
                => state.WithChange(new QueryUserChange(state.Data));
        }

        [BelogsToState(typeof(UsersStade))]
        public static class UsersQueryReducer
        {
            [Reducer]
            public static MutatingContext<UserList> QueryUsers(MutatingContext<UserList> state, QueryUsersAction action)
                => state.WithChange(new QueryUsersChange(state.Data));
        }

        [Effect]
        public sealed class TestEffect : IEffect
        {
            public void Handle(IStateAction action, IActionInvoker dispatcher) => Console.WriteLine($"Hello From Effect: {action}");

            public bool ShouldReactToAction(IStateAction action) => true;
        }

        public sealed class InteractionActor : ReceiveActor
        {
            private readonly IActionInvoker _manager;
            
            private string _lastId = string.Empty;
            private bool _firstCall = true;

            public InteractionActor(IActionInvoker invoker)
            {
                _manager = invoker;
                Receive<NotUsed>(InvokeNext);
                Receive<OperationResult>(r => Console.WriteLine($"Operation Result: {r.Ok}"));
                Receive<string>(Input);
            }

            private void Input(string line)
            {
                switch (line)
                {
                    case "kill":
                        _manager.Run(new KillApplicationAction());
                        return;
                    case "neu":
                        _lastId = Guid.NewGuid().ToString("N");
                        _manager.Run(new CreateUserAction(_lastId));
                        break;
                    case "name":
                        Console.Write("Name: ");
                        var name = Console.ReadLine()!;
                        _manager.Run(new RenameUserAction(_lastId, name));
                        break;
                    case "daten":
                        Console.Write("Name oder Id: ");
                        var id = Console.ReadLine();
                        _manager.Run(new QueryUserAction(id!));
                        break;
                    case "liste":
                        _manager.Run(new QueryUsersAction());
                        break;
                    case "löschen":
                        Console.Write("Name oder Id: ");
                        var delId = Console.ReadLine();
                        _manager.Run(new DeleteUserAction(delId!));
                        break;
                    default:
                        Console.WriteLine("Unbekanntes Commando");
                        break;
                }

                Self.Tell(NotUsed.Instance);
            }

            private void InvokeNext(NotUsed _)
            {
                if (_firstCall)
                {
                    Console.WriteLine("Fertig");
                    _firstCall = false;

                    Console.WriteLine();
                }
                Task.Run(Console.ReadLine).PipeTo(Self);
            }
        }

        private static async Task Main(string[] args)
        {
            var actorsystem = ActorSystem.Create("TestSystem", ConfigurationFactory.ParseString("akka.loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"));

            var testContainerBuilder = new ContainerBuilder();

            //var superwiser = new WorkspaceSuperviser(actorsystem, "Workspace");
            //var testManager = ManagerBuilder.CreateManager(superwiser, builder =>

            testContainerBuilder.RegisterType<UserSourceFactory>().As<IDataSourceFactory>().SingleInstance();
            testContainerBuilder.RegisterType<KillState>().AsSelf();
            testContainerBuilder.RegisterInstance(actorsystem).As<ActorSystem>();
            testContainerBuilder.RegisterStateManager((builder, context) =>
            {
                builder.WithConsistentHashDispatcher().NrOfInstances(4);

                builder.WithDefaultSendback(true);
                builder.WithGlobalCache(p => p.WithSystemRuntimeCacheHandle());

                builder.AddFromAssembly<InteractionActor>(context);
            });


            await using var testContiner = testContainerBuilder.Build();
            var testManager = testContiner.Resolve<IActionInvoker>();

            var userState = testManager.GetState<UserStade>()!;
            userState.OnChange.RespondOn(d => Console.WriteLine($"User Data Changed: {d.Id}"));
            userState.QueryUserName.RespondOn(q => Console.WriteLine($"Benutzer Abgefragt: \n {q.Data}"));
            userState.UserCreated.RespondOn(c => Console.WriteLine($"Benutzer Estellt: {c.Id}"));
            userState.UserRenamed.RespondOn(n => Console.WriteLine($"Benutzer Umbenannt: {n.Id} -- {n.NewName}"));

            var usersStade = testManager.GetState<UsersStade>()!;
            usersStade.QueryUsers.RespondOn(q =>
            {
                foreach (var user in q.Users) 
                    Console.WriteLine($"Benutzer {user.Name} -- {user.Id}");
            });

            // ReSharper disable once AccessToDisposedClosure
            actorsystem.ActorOf(() => new InteractionActor(testManager), "Interaction_System").Tell(NotUsed.Instance);

            await actorsystem.WhenTerminated;
            actorsystem.Dispose();
        }
    }
}