using Tauron.Application.Workshop.Analyzing.Rules;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Analyzing
{
    public interface IAnalyzer
    {
        IEventSource<IssuesEvent> Issues { get; }
    }

    public interface IAnalyzer<TWorkspace, TData> : IAnalyzer
        where TWorkspace : WorkspaceBase<TData> where TData : class
    {
        void RegisterRule(IRule<TWorkspace, TData> rule);
    }
}