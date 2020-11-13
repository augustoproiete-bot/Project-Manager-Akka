using System;
using System.Globalization;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Extension
{
    [PublicAPI]
    public sealed class LocExtensionAdaptor
    {
        private readonly LocExtension _extension;
        private readonly ActorSystem _system;

        public LocExtensionAdaptor(LocExtension extension, ActorSystem system)
        {
            _system = system;
            _extension = Argument.NotNull(extension, nameof(extension));
        }

        public void Request(string name, Action<object?> valueResponse, Maybe<CultureInfo> info = default)
        {
            var hook = EventActor.Create(_system, Maybe<string>.Nothing, true);
            hook.Do(h =>
            {
                h.Register(HookEvent.Create<LocCoordinator.ResponseLocValue>(res => valueResponse(res.Result)));
                h.Send(_extension.LocCoordinator, new LocCoordinator.RequestLocValue(name, info.Or(CultureInfo.CurrentUICulture)));
            });
        }

        public Maybe<object> Request(string name, Maybe<CultureInfo> info = default) 
            => _extension.LocCoordinator.Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info.Or(CultureInfo.CurrentUICulture))).Result.Result;

        public Task<Maybe<object>> RequestTask(string name, Maybe<CultureInfo> info = default) 
            => _extension.LocCoordinator.Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info.Or(CultureInfo.CurrentUICulture)))
                .ContinueWith(t => t.Result.Result);

        public Maybe<string> RequestString(string name, Maybe<CultureInfo> info = default)
        {
            return (from o in  Request(name, info)
                select o.ToString() ?? string.Empty)!;
        }

        public void RequestString(string name, Action<string> valueResponse, Maybe<CultureInfo> info = default) 
            => Request(name, o => valueResponse(o?.ToString() ?? string.Empty), info);
    }
}