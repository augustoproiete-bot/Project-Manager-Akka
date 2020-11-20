using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Tauron.Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class BuildActorCoordinator : StatefulReceiveActor<BuildActorCoordinator.CoordinatorState>
    {
        public record CoordinatorState(int Remaining, bool Fail, IActorRef Agents);

        public BuildActorCoordinator()
            : base(new CoordinatorState(0, false, CreateAgentPool()))
        {
            Flow<BuildRequest>(b => b.Action(ProcessRequest));
            Flow<AgentCompled>(b => b.Func(SingleBuildCompled).ToSender());
        }

        private static IActorRef CreateAgentPool()
            => Context.ActorOf(Props.Create<BuildAgent>().WithRouter(new SmallestMailboxPool(Environment.ProcessorCount * 2)), "Agents");

        private Maybe<BuildCompled> SingleBuildCompled(Maybe<AgentCompled> arg)
        {
            return
                from _ in MayRun(s =>
                    from state in s
                    select state with{Remaining = state.Remaining - 1})

                from agent in arg
                from state in MayRun(s =>
                    from state in s
                    where agent.Failed
                    from error in agent.Cause
                    from _ in To(Log).Warning(error, "Error in Build Agent")
                    select state with{Fail = true})

                where state.Remaining == 0
                select new BuildCompled(agent.OperationId, state.Fail);
        }

        private Maybe<Unit> ProcessRequest(Maybe<BuildRequest> obj)
            =>
                from request in obj
                from m1 in MayTell(Context.Sender, BuildMessage.GatherData(request.OperationId))
                from reset in MayRun(s =>
                    from state in s
                    select state with{Remaining = 0, Fail = false})

                let data = request.ProjectFile
                let toBuild = ImmutableList<PreparedBuild>.Empty.AddRange(
                    from project in data.Projects
                    let path = data.FindProjectPath(May(project))
                    where !string.IsNullOrWhiteSpace(path.OrElseDefault())
                    select new PreparedBuild(data.BuildInfo, project, data, request.OperationId, path.Value))

                from count in
                    MayTell(
                        BuildMessage.NoData(request.OperationId),
                        from actor in MayActor(Context.Sender)
                        where toBuild.Count == 0
                        select actor)

                where toBuild.Count != 0
                from agents in MayRun(s =>
                    from state in s
                    from start in MayAction(() => toBuild.ForEach(pr => Tell(state.Agents, pr)))
                    select state with{Remaining = toBuild.Count})
                select Unit.Instance;
    }
}