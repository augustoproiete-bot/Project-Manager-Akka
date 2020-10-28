using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using CacheManager.Core;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace AkkaTest
{
    internal static class Program
    {
        public sealed class UserData : IChangeTrackable
        {
            public string Id { get; }

            public string Name { get; }

            public DateTime LastAccess { get; }

            public DateTime CreationTime { get; }

            public bool IsChanged { get; }

            public bool IsNew { get; }

            public UserData(string name, DateTime lastAccess, DateTime creationTime, bool isChanged, bool isNew, string id)
            {
                Name = name;
                LastAccess = lastAccess;
                CreationTime = creationTime;
                IsChanged = isChanged;
                IsNew = isNew;
                Id = id;
            }

            public UserData CreateFromNew() => !IsNew ? this : new UserData(Name, LastAccess, CreationTime, true, false, Name);

            public UserData UpdateName(string name)
                => new UserData(name, DateTime.Now, CreationTime, true, false, Id);

            public UserData SetUnchanged()
                => new UserData(Name, LastAccess, CreationTime, false, false, Id);
        }

        public sealed class StringQuery : IQuery
        {
            public string Name { get; }

            public StringQuery(string name) => Name = name;

            public object ToHash() => Name;
        }

        public sealed class EmptyQuery : IQuery
        {
            public object ToHash() => "Empty";
        }

        public sealed class KillQuery : IQuery
        {
            public object ToHash() => "Kill";
        }

        public sealed class UserDataSource : IStateDataSource<UserData>
        {
            private static readonly UserData Empty = new UserData(string.Empty, DateTime.MinValue, DateTime.MinValue, false, false, string.Empty);

            private readonly ConcurrentDictionary<string, UserData> _user;
            private UserData _next = Empty;

            public UserDataSource(ConcurrentDictionary<string, UserData> user) => _user = user;

            public UserData GetData() => _next;

            public void SetData(UserData data) => _user.AddOrUpdate(data.Name, data, (s, userData) => data.SetUnchanged());

            public void Apply(IQuery query)
            {
                if (query is StringQuery info)
                    _next = _user.GetOrAdd(info.Name, s => new UserData(s, DateTime.Now, DateTime.Now, true, true, s));
                else
                    _next = Empty;
            }
        }

        public sealed class UsersDataSource : IStateDataSource<ICollection<UserData>>
        {
            private readonly ConcurrentDictionary<string, UserData> _userDatas;

            public UsersDataSource(ConcurrentDictionary<string, UserData> userDatas) => _userDatas = userDatas;

            public ICollection<UserData> GetData() => _userDatas.Values;

            public void SetData(ICollection<UserData> data) { }

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

        public sealed class QueryUserNameAction : IStateAction
        {
            public string ActionName => "QueryUserName";
            public IQuery Query { get; }

            public QueryUserNameAction(string query) => Query =new StringQuery(query);
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

        public sealed class QueryUsersChange
        {
            
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

        public sealed class KillState : IState<UserData>
        {
            public KillState(MutatingEngine<MutatingContext<UserData>> engine)
            {
                Kill = engine.EventSource<UserData, KillChange>();
            }

            public IEventSource<KillChange> Kill { get; }
        }

        public sealed class UserStade : IState<UserData>
        {
            public UserStade(MutatingEngine<MutatingContext<UserData>> engine)
            {
                QueryUserName = engine.EventSource<UserData, QueryUserChange>();
                UserRenamed = engine.EventSource<UserData, RenameChange>();
                UserCreated = engine.EventSource<UserData, CreateUserChange>();
            }

            public IEventSource<QueryUserChange> QueryUserName { get; }

            public IEventSource<RenameChange> UserRenamed { get; }

            public IEventSource<CreateUserChange> UserCreated { get; }
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

            public bool ShouldReduceStateForAction(IStateAction action) => action is QueryUserNameAction;
        }

        private static async Task Main(string[] args)
        {
            var actorsystem = ActorSystem.Create("TestSystem");

            var superwiser = new WorkspaceSuperviser(actorsystem);
            var testManager = ManagerBuilder.CreateManager(superwiser, builder =>
            {
                var database = new ConcurrentDictionary<string, UserData>();
                var source = new UserDataSource(database);

                builder.WithConsistentHashDispatcher().NrOfInstances(4);

                builder.WithGlobalCache(p => p.WithSystemRuntimeCacheHandle());
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
                   .WithReducer(() => new RenameUserreducer());
            });

            var killStade = testManager.GetState<KillState>()!;
            killStade.Kill.RespondOn(k => actorsystem.Terminate());

            var userState = testManager.GetState<UserStade>()!;

            userState.QueryUserName.RespondOn(q => Console.WriteLine($"User Name Queried: {q.Data.Name}"));
            userState.UserCreated.RespondOn(c => Console.WriteLine($"User Created: {c.Id}"));
            userState.UserRenamed.RespondOn(n => Console.WriteLine($"User Renamed {n.Id} -- {n.NewName}"));
            
            await Task.Run(() => StartLoop(testManager));

            await actorsystem.WhenTerminated;
            testManager.Dispose();
            actorsystem.Dispose();
        }

        private static void StartLoop(RootManager manager)
        {
            bool run = true;

            do
            {
                var line = Console.ReadLine();

                switch (line)
                {
                    case "kill":
                        run = false;
                        manager.Run(new KillApplicationAction());
                        break;
                    default:
                        Console.WriteLine("Unbekanntes Commando");
                        break;
                }

            } while (run);
        }
    }
}