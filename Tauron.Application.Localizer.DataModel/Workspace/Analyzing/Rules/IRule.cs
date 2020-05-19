using System.Collections.Generic;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules
{
    public interface IRule
    {
        string Name { get; }

        IActorRef Init(IActorRefFactory superviser, ProjectFileWorkspace workspace);
    }
}