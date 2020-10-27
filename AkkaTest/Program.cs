using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Accessibility;
using Akka.TestKit;
using CacheManager.Core;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;

namespace AkkaTest
{
    internal class Program : TestKitBase
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

        public sealed class UserDataSource : IStateDataSource<UserData>
        {
            private readonly ConcurrentDictionary<string, UserData> _user = new ConcurrentDictionary<string, UserData>();
            private UserData _next = new UserData(string.Empty, DateTime.MinValue, DateTime.MinValue, false, false, string.Empty);

            public UserData GetData() => _next;

            public void SetData(UserData data) => _user.AddOrUpdate(data.Name, data, (s, userData) => data.SetUnchanged());

            public void Apply(string query) => _next = _user.GetOrAdd(query, s => new UserData(s, DateTime.Now, DateTime.Now, true, true, s));
        }

        public sealed class CreateUserAction : IStateAction
        {
            public string ActionName => "CreateUser";

            public string Query { get; }

            public CreateUserAction(string userId) => Query = userId;
        }

        public sealed class RenameUserAction : IStateAction
        {
            public string ActionName => "RenameUser";

            public string Query { get; }

            public string NewName { get; }

            public RenameUserAction(string query, string newName)
            {
                Query = query;
                NewName = newName;
            }
        }

        public sealed class QueryUserNameAction : IStateAction
        {
            public string ActionName => "QueryUserName";
            public string Query { get; }

            public QueryUserNameAction(string query) => Query = query;
        }

        public sealed class KillApplicationAction : IStateAction
        {
            public string ActionName => "KillApp";
            public string Query { get; }

            public KillApplicationAction()
            {
                Query = "Kill";
            }
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

        private sealed class Asserations : ITestKitAssertions
        {
            public void Fail(string format = "", params object[] args)
            {
                
            }

            public void AssertTrue(bool condition, string format = "", params object[] args)
            {
            }

            public void AssertFalse(bool condition, string format = "", params object[] args)
            {
            }

            public void AssertEqual<T>(T expected, T actual, string format = "", params object[] args)
            {
            }

            public void AssertEqual<T>(T expected, T actual, Func<T, T, bool> comparer, string format = "", params object[] args)
            {
            }
        }

        public Program()
            : base(new Asserations())
        {
               
        }

        public void RunTest()
        {
            var superwiser = new WorkspaceSuperviser(Sys);
            var testManager = ManagerBuilder.CreateManager(superwiser, builder =>
            {
                var source = new UserDataSource();

                builder.WithConsistentHashDispatcher().WithCustomization(p => p.WithDispatcher(CallingThreadDispatcher.Id));

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
            killStade.Kill.RespondOn(k => Sys.Terminate());

            var userState = testManager.GetState<UserStade>()!;

            userState.QueryUserName.RespondOn(q => Console.WriteLine($"User Name Queried: {q.Data.Name}"));
            userState.UserCreated.RespondOn(c => Console.WriteLine($"User Created: {c.Id}"));
            userState.UserRenamed.RespondOn(n => Console.WriteLine($"User Renamed {n.Id} -- {n.NewName}"));

            var userId = Guid.NewGuid().ToString("N");

            testManager.Run(new CreateUserAction(userId));
            testManager.Run(new RenameUserAction(userId, "Tauron"));
            testManager.Run(new QueryUserNameAction(userId));
            testManager.Run(new KillApplicationAction());
        }

        private static void Main(string[] args)
        {
            var prog = new Program();

            Task.Run(prog.RunTest);

            Console.ReadKey();
        }
    }
}