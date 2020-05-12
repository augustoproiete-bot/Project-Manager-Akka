using System;
using System.Globalization;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization;

namespace Tauron.Application.Localizer.temp.Prototype
{
    [PublicAPI]
    public sealed class LocPrototype
    {
        public LocPrototype(ActorSystem system, CultureInfo? info = null)
        {
            var loc = system.Loc();

            _test = loc.RequestTask("project-test", info).ContinueWith(t => t.Result as string);
            _test2 = loc.RequestTask("project-test2", info).ContinueWith(t => t.Result as string);
        }

        private readonly Task<string?> _test;
        public string Test => _test.Result ?? string.Empty;

        private readonly Task<string?> _test2;

        public int Test2 => Convert.ToInt32(_test2.Result);
    }
}