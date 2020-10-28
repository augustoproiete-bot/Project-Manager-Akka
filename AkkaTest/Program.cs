using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using CacheManager.Core;
using Tauron;
using Tauron.Akka;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace AkkaTest
{
    internal static class Program
    {
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

        public sealed class UserData : IChangeTrackable, IStateEntity
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

            public UserData CreateFromNew() => !IsNew ? this : new UserData(Name, LastAccess, CreationTime, true, false, Name, false);

            public UserData UpdateName(string name)
                => new UserData(name, DateTime.Now, CreationTime, true, false, Id, false);

            public UserData SetUnchanged()
                => new UserData(Name, LastAccess, CreationTime, false, false, Id, false);

            public UserData MarkDelete()
                => new UserData(Name, DateTime.Now, CreationTime, true, false, Id, true);

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

        public sealed class UserDataSource : IStateDataSource<UserData>
        {
            private static readonly UserData Empty = new UserData(string.Empty, DateTime.MinValue, DateTime.MinValue, false, false, string.Empty, true);

            private readonly ConcurrentDictionary<string, UserData> _user;
            private UserData? _next;

            public UserDataSource(ConcurrentDictionary<string, UserData> user) => _user = user;

            public UserData GetData() => _next ?? Empty;

            public void SetData(UserData data)
            {
                if (data.IsDeleted)
                    _user.TryRemove(data.Id, out _);
                else
                    _user.AddOrUpdate(data.Id, data, (s, userData) => data.SetUnchanged());
            }

            public void Apply(IQuery query)
            {
                switch (query)
                {
                    case UserQuery q:
                        if(_user.TryGetValue(q.Name, out _next))
                            return;
                        _next = _user.Values.FirstOrDefault(u => u.Name == q.Name);
                        break;
                    case StringQuery info:
                        _next = _user.GetOrAdd(info.Name, s => new UserData(s, DateTime.Now, DateTime.Now, true, true, s, false));
                        break;
                    default:
                        _next = Empty;
                        break;
                }
            }
        }

        public sealed class UsersDataSource : IStateDataSource<UserList>
        {
            private readonly ConcurrentDictionary<string, UserData> _userDatas;

            public UsersDataSource(ConcurrentDictionary<string, UserData> userDatas) => _userDatas = userDatas;

            public UserList GetData() => new UserList(_userDatas.Values);

            public void SetData(UserList data) { }

            public void Apply(IQuery query) { }
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

        public sealed class KillState : StateBase<UserData>
        {
            public KillState(MutatingEngine<MutatingContext<UserData>> engine)
                : base(engine)
            {
                Kill = engine.EventSource<UserData, KillChange>();
            }

            public IEventSource<KillChange> Kill { get; }
        }

        public sealed class UserStade : StateBase<UserData>
        {
            public UserStade(MutatingEngine<MutatingContext<UserData>> engine)
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

        public sealed class UsersStade : StateBase<UserList>
        {
            public UsersStade(MutatingEngine<MutatingContext<UserList>> engine) 
                : base(engine)
                => QueryUsers = engine.EventSource<UserList, QueryUsersChange>();

            public IEventSource<QueryUsersChange> QueryUsers { get; }
        }

        public sealed class KillReducer : IReducer<UserData>
        {
            public MutatingContext<UserData> Reduce(MutatingContext<UserData> state, IStateAction action) => state.WithChange(new KillChange());

            public bool ShouldReduceStateForAction(IStateAction action) => action is KillApplicationAction;
        }

        public sealed class CreateUserReducer : IReducer<UserData>
        {
            public MutatingContext<UserData> Reduce(MutatingContext<UserData> state, IStateAction action) 
                => state.Update(new CreateUserChange(state.Data.Name), state.Data.CreateFromNew());

            public bool ShouldReduceStateForAction(IStateAction action) 
                => action is CreateUserAction;
        }

        public sealed class RenameUserreducer : IReducer<UserData>
        {
            public MutatingContext<UserData> Reduce(MutatingContext<UserData> state, IStateAction action) 
                => state.Update(new RenameChange(((RenameUserAction)action).NewName, state.Data.Id), state.Data.UpdateName(((RenameUserAction) action).NewName));

            public bool ShouldReduceStateForAction(IStateAction action) => action is RenameUserAction;
        }

        public sealed class QueryUserNameReducer : IReducer<UserData>
        {
            public MutatingContext<UserData> Reduce(MutatingContext<UserData> state, IStateAction action) => state.WithChange(new QueryUserChange(state.Data));

            public bool ShouldReduceStateForAction(IStateAction action) => action is QueryUserAction;
        }

        public sealed class QueryUsersReducer : IReducer<UserList>
        {
            public MutatingContext<UserList> Reduce(MutatingContext<UserList> state, IStateAction action) => state.WithChange(new QueryUsersChange(state.Data));

            public bool ShouldReduceStateForAction(IStateAction action) => action is QueryUsersAction;
        }

        public sealed class DeleteUserReducer : IReducer<UserData>
        {
            public MutatingContext<UserData> Reduce(MutatingContext<UserData> state, IStateAction action) 
                => state.Update(new DeleteUserChange(state.Data), state.Data.MarkDelete());

            public bool ShouldReduceStateForAction(IStateAction action) => action is DeleteUserAction;
        }

        private static async Task Main(string[] args)
        {
            var actorsystem = ActorSystem.Create("TestSystem", ConfigurationFactory.ParseString("akka.loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"));

            var superwiser = new WorkspaceSuperviser(actorsystem, "Workspace");
            var testManager = ManagerBuilder.CreateManager(superwiser, builder =>
            {
                var database = new ConcurrentDictionary<string, UserData>();
                var source = new UserDataSource(database);
                var usersSource = new UsersDataSource(database);

                builder.WithConsistentHashDispatcher().NrOfInstances(4);

                builder.WithGlobalCache(p => p.WithSystemRuntimeCacheHandle());

                builder.WithDataSource(() => usersSource)
                    .WithStateType<UsersStade>()
                    .WithReducer(() => new QueryUsersReducer());

                builder.WithDataSource(() => source)
                   .WithParentCache()
                   .WithStateType<KillState>()
                   .WithReducer(() => new KillReducer());

                builder.WithDataSource(() => source)
                    .WithCache(p => p.WithDictionaryHandle())
                    .WithParentCache()
                    .WithStateType<UserStade>()
                    .WithReducer(() => new CreateUserReducer())
                    .WithReducer(() => new QueryUserNameReducer())
                    .WithReducer(() => new RenameUserreducer())
                    .WithReducer(() => new DeleteUserReducer());
            });

            var killStade = testManager.GetState<KillState>()!;
            killStade.Kill.RespondOn(k => ExposedReceiveActor.ExposedContext.System.Terminate());

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

            await Task.Run(() => StartLoop(testManager));

            await actorsystem.WhenTerminated;
            testManager.Dispose();
            actorsystem.Dispose();
        }

        private static void StartLoop(IActionInvoker manager)
        {
            var run = true;
            string lastId = string.Empty;

            do
            {
                var line = Console.ReadLine();

                switch (line)
                {
                    case "kill":
                        run = false;
                        manager.Run(new KillApplicationAction());
                        break;
                    case "neu":
                        lastId = Guid.NewGuid().ToString("N");
                        manager.Run(new CreateUserAction(lastId));
                        break;
                    case "name":
                        Console.Write("Name: ");
                        var name = Console.ReadLine()!;
                        manager.Run(new RenameUserAction(lastId, name));
                        break;
                    case "daten":
                        Console.Write("Name oder Id: ");
                        var id = Console.ReadLine();
                        manager.Run(new QueryUserAction(id!));
                        break;
                    case "liste":
                        manager.Run(new QueryUsersAction());
                        break;
                    case "löschen":
                        Console.Write("Name oder Id: ");
                        var delId = Console.ReadLine();
                        manager.Run(new DeleteUserAction(delId!));
                        break;
                    default:
                        Console.WriteLine("Unbekanntes Commando");
                        break;
                }

            } while (run);
        }
    }
}