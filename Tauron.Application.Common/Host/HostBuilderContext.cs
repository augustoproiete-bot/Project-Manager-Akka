using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    public sealed class HostBuilderContext
    {
        public IHostingEnvironment HostingEnvironment
        {
            get;
            set;
        }

        public IConfiguration Configuration
        {
            get;
            set;
        }

        public IDictionary<object, object> Properties
        {
            get;
        }

        public HostBuilderContext(IDictionary<object, object> properties, IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
            Properties = (properties ?? throw new ArgumentNullException(nameof(properties)));
        }
    }
}