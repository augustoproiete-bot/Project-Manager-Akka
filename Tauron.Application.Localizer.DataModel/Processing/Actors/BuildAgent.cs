using System;
using Tauron.Akka;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class BuildAgent : ExposedReceiveActor
    {
        public BuildAgent()
        {
            this.Flow<PreparedBuild>()
                .To.Func(OnBuild).Forward.ToParent();
        }

        private AgentCompled OnBuild(PreparedBuild build)
        {
            try
            {
                var agentName = Context.Self.Path.Name;

                return new AgentCompled(false, null, build.Operation);
            }
            catch (Exception e)
            {
                return new AgentCompled(true, e, build.Operation);
            }
        }
    }
}