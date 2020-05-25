using Tauron.Application.Workshop.Analyzing.Rules;

namespace Tauron.Application.Workshop.Analyzing.Actor
{
    public sealed class RegisterRule<TWorkspace, TData>
        where TWorkspace : WorkspaceBase<TData>
    {
        public IRule<TWorkspace, TData> Rule { get; }

        public RegisterRule(IRule<TWorkspace, TData> rule) 
            => Rule = rule;
    }
}