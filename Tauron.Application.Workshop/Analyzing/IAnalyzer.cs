using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing
{
    public interface IAnalyzer
    {
        IEventSource<IssuesEvent> Issues { get; }
    }

    public interface IAnalyzer<out TWorkspace, TData> : IAnalyzer
        where TWorkspace : WorkspaceBase<TData>
    {
        void RegisterRule(IRule<TWorkspace, TData> rule);
    }
}