using System;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Tauron.Host
{
    public sealed class CommonLifetime : IHostLifetime
    {
        private readonly string _appRoute;
        private readonly IComponentContext _factory;
        private readonly ILogger _logger = Log.Logger.ForContext<CommonLifetime>();

        private IAppRoute? _route;

        public CommonLifetime(IConfiguration configuration, IComponentContext factory)
        {
            _factory = factory;
            _appRoute = configuration.GetValue("route", "default");
        }

        public async Task WaitForStartAsync(ActorSystem actorSystem)
        {
            _logger.Information("Begin Start Application");
            try
            {
                string name = !string.IsNullOrEmpty(_appRoute) ? _appRoute : "default";
                _logger.Information("Try get Route for {RouteName}", name);

                _route = GetRoute(name);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Error on get Route");
                _route = GetRoute("default");
            }

            await _route.WaitForStartAsync(actorSystem);
            ShutdownTask = _route.ShutdownTask;
        }

        public Task ShutdownTask { get; private set; } = Task.CompletedTask;

        private IAppRoute GetRoute(string name)
        {
            return _factory.ResolveNamed<IAppRoute>(name);
        }
    }
}