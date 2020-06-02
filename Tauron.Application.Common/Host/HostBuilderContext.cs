using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    public sealed class HostBuilderContext
    {
        public HostBuilderContext(IDictionary<object, object> properties, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            HostEnvironment = hostEnvironment;
            Configuration = configuration;
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public IHostEnvironment HostEnvironment { get; set; }

        public IConfiguration Configuration { get; set; }

        public IDictionary<object, object> Properties { get; }
    }
}