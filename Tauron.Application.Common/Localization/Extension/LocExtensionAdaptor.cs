using System;
using System.Globalization;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Extension
{
    [PublicAPI]
    public sealed class LocExtensionAdaptor
    {
        private readonly ActorSystem _system;
        private readonly LocExtension _extension;

        public LocExtensionAdaptor(LocExtension extension, ActorSystem system)
        {
            _system = system;
            _extension = Argument.NotNull(extension, nameof(extension));
        }

        public void Request(string name, Action<object?> valueResponse, CultureInfo? info = null)
        {
            var hook = EventActor.Create(_system, true);
            hook.Register(HookEvent.Create<LocCoordinator.ResponseLocValue>(res => valueResponse(res.Result)));
            hook.Send(_extension.LocCoordinator, new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture));
        }
    }
}