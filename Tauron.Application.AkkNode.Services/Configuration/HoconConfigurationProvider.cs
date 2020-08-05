using System;
using System.Collections.Generic;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;

namespace Tauron.Application.AkkNode.Services.Configuration
{
    public sealed class HoconConfigurationSource : IConfigurationSource
    {
        private readonly Func<Config> _config;
        private readonly (string path, string name)[] _names;

        public HoconConfigurationSource(Func<Config> config, params (string path, string name)[] names)
        {
            _config = config;
            _names = names;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) 
            => new HoconConfigurationProvider(_config, _names);
    }

    public sealed class HoconConfigurationProvider : ConfigurationProvider
    {
        private Func<Config>? _config;
        private (string path, string name)[]? _names;

        public HoconConfigurationProvider(Func<Config> config, params (string path, string name)[] names)
        {
            _config = config;
            _names = names;
        }

        public override void Load()
        {
            if(_config == null || _names == null)
                return;

            var config = _config();

            foreach (var (path, name) in _names)
            {
                if (config.HasPath(path))
                    Data[name] = config.GetString(path, string.Empty);
            }

            _names = null;
            _config = null;
        }
    }
}