using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules;

namespace Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Actor
{
    public sealed class RegisterRule
    {
        public IRule Rule { get; }

        public RegisterRule(IRule rule) 
            => Rule = rule;
    }
}