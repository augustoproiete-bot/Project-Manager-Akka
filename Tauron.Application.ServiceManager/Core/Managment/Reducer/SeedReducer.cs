using System;
using Akka.Actor;
using FluentValidation;
using FluentValidation.Validators;
using Tauron.Application.Localizer.Generated;
using Tauron.Application.ServiceManager.Core.Managment.Data;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.ServiceManager.Core.Managment.Reducer
{
    [BelogsToState(typeof(SeedState))]
    public static class SeedReducer
    {
        [Reducer]
        public static MutatingContext<ClusterConfiguration> TryJoinReducer(MutatingContext<ClusterConfiguration> state, TryJoinAction action)
        {
            var seeds = state.Data.Seeds;
            if (seeds.Count == 0)
                seeds = seeds.Add(state.Data.SelfAddress);

            return state.WithChange(new TryJoinEvent(seeds));
        }

        [Validator]
        public static IValidator<AddSeedUrlAction> AddSeedValidator { get; } = new AddSeedUrlActionValidator();

        [Reducer]
        public static MutatingContext<ClusterConfiguration> AddSeedUrl(MutatingContext<ClusterConfiguration> state, AddSeedUrlAction action) 
            => state.Data.Seeds.Contains(action.Url) ? state : state.WithChange(new AddSeedUrlEvent(action.Url));

        [Validator]
        public static IValidator<RemoveSeedUrlAction> RemoveSeedValidator { get; } = new RemoveSeedUrlActionValidator();

        [Reducer]
        public static MutatingContext<ClusterConfiguration> RemoveSeedUlr(MutatingContext<ClusterConfiguration> state, RemoveSeedUrlAction action) 
            => !state.Data.Seeds.Contains(action.Url) ? state : state.WithChange(new RemoveSeedUrlEvent(action.Url, state.Data.Seeds.Count - 1));

        private sealed class AddSeedUrlActionValidator : AbstractValidator<AddSeedUrlAction>
        {
            public AddSeedUrlActionValidator() 
                => RuleFor(a => a.Url).NotEmpty().Custom(UrlValidator.ValidateUrl);
        }

        private sealed class RemoveSeedUrlActionValidator : AbstractValidator<RemoveSeedUrlAction>
        {
            public RemoveSeedUrlActionValidator() 
                => RuleFor(r => r.Url).NotEmpty().Custom(UrlValidator.ValidateUrl);
        }

        private static class UrlValidator
        {
            public static void ValidateUrl(string url, CustomContext context)
            {
                try
                {
                    Address.Parse(url);
                }
                catch (Exception e)
                {
                    context.AddFailure(e.Message);
                    context.AddFailure(LocLocalizer.Inst.SeedProcessor.AddSeedUrlInvalidFromat);
                }
            }
        }
    }
}