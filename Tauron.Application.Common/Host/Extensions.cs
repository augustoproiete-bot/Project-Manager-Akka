using System;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    [PublicAPI]
    public static class Extensions
    {
        public static IRegistrationBuilder<TType, SimpleActivatorData, SingleRegistrationStyle> Configure<TType>(this ContainerBuilder builder, string? name = null)
            where TType : notnull, new()
        {
            return builder.Register(c =>
                                    {
                                        var appConfig = c.Resolve<IConfiguration>();
                                        var config = new TType();
                                        if (string.IsNullOrWhiteSpace(name))
                                            appConfig.Bind(config);
                                        else
                                            appConfig.Bind(name, config);

                                        return config;
                                    });
        }

        public static IRegistrationBuilder<TType, SimpleActivatorData, SingleRegistrationStyle> Configure<TType>(this ContainerBuilder builder, Action<TType> configAction)
            where TType : notnull, new()
        {
            return builder.Register(c =>
                                    {
                                        var config = new TType();
                                        configAction(config);

                                        return config;
                                    });
        }
    }
}