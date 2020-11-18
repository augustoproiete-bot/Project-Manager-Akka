using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    [PublicAPI]
    public static class HostExtensions
    {
        public static IApplicationBuilder UseContentRoot(this IApplicationBuilder hostBuilder, string contentRoot)
        {
            return hostBuilder.Configuration(configBuilder =>
                                             {
                                                 configBuilder
                                                    .AddInMemoryCollection(new[]
                                                                           {
                                                                               new KeyValuePair<string, string>(HostDefaults.ContentRootKey, contentRoot
                                                                                                                                          ?? throw new ArgumentNullException(nameof(contentRoot)))
                                                                           });
                                             });
        }
    }
}