﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public static class ManagerBuilderExtensions
    {
        private sealed class Buildhelper
        {
            public static readonly ParameterInfo[] Parameters = typeof(Buildhelper).GetConstructors().First().GetParameters();

            public static object GetParam(ParameterInfo info, IComponentContext context, Func<object> alternative, IEnumerable<Parameter> param)
            {
                Func<object?>? factory = null;

                foreach (var parameter in param)
                {
                    if (parameter.CanSupplyValue(info, context, out factory))
                        break;
                }

                factory ??= alternative;

                return factory() ?? alternative();
            }

            private WorkspaceSuperviser Superviser { get; }
            private Action<ManagerBuilder, Maybe<IComponentContext>> Action { get; }

            public Buildhelper(WorkspaceSuperviser superviser, Action<ManagerBuilder, Maybe<IComponentContext>> action)
            {
                Superviser = superviser;
                Action = action;
            }

            public RootManager Create(Maybe<IComponentContext> context, Maybe<AutofacOptions> autofacOptions)
            {
                var config = new ManagerBuilder(Superviser);
                Action(config, context);

                return config.Build(context, autofacOptions);
            }
        }

        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, Action<ManagerBuilder, Maybe<IComponentContext>> configAction) 
            => RegisterStateManager(builder, true, configAction);

        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, bool registerWorkspaceSuperviser, Action<ManagerBuilder, Maybe<IComponentContext>> configAction) 
            => RegisterStateManager(builder, new AutofacOptions {RegisterSuperviser = registerWorkspaceSuperviser}, configAction);

        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, AutofacOptions options, Action<ManagerBuilder, Maybe<IComponentContext>> configAction)
        {
            static bool ImplementInterface(Type target, Type interfac) 
                => target.GetInterface(interfac.Name) != null;

            if (options.AutoRegisterInContainer)
            {
                builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(
                    t => t.IsAssignableTo<IState>()     || t.IsAssignableTo<IEffect>() ||
                        t.IsAssignableTo<IMiddleware>() || ImplementInterface(t, typeof(IReducer<>))));
            }

            if (options.RegisterSuperviser)
                builder.Register(c => new WorkspaceSuperviser(c.Resolve<ActorSystem>(), "State_Manager_Superviser")).AsSelf().SingleInstance();

            builder.Register((context, parameters) =>
            {
                var supplyedParameters = parameters.ToArray();
                object[] param = new object[2];
                param[0] = Buildhelper.GetParam(Buildhelper.Parameters[0], context, () => context.Resolve(typeof(WorkspaceSuperviser)), supplyedParameters);
                param[1] = Buildhelper.GetParam(Buildhelper.Parameters[1], context, () => configAction, supplyedParameters);

                return (Activator.CreateInstance(typeof(Buildhelper), param) as Buildhelper ?? throw new InvalidOperationException("Build Helper Creation Failed"))
                        .Create(context.ToMaybe(), options.ToMaybe());
            }).As<IActionInvoker>().SingleInstance();

            return builder;
        }

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IDataSourceFactory factory, Maybe<IComponentContext> context = default)
            => AddFromAssembly(builder, typeof(TType).Assembly, factory, context);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IDataSourceFactory factory, Maybe<IComponentContext> context= default)
        {
            new ReflectionSearchEngine(assembly, context).Add(builder, factory);
            return builder;
        }

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IComponentContext context)
            => AddFromAssembly(builder, typeof(TType).Assembly, context);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IComponentContext context) 
            => AddFromAssembly(builder, assembly, MergeFactory.Merge(context.Resolve<IEnumerable<IDataSourceFactory>>().Cast<AdvancedDataSourceFactory>()), context.ToMaybe());

        public static IConcurrentDispatcherConfugiration WithConcurentDispatcher(this ManagerBuilder builder)
        {
            var config = new ConcurrentDispatcherConfugiration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher(this ManagerBuilder builder)
        {
            var config = new ConsistentHashDispatcherConfiguration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static TConfig WithDefaultConfig<TConfig>(this IDispatcherPoolConfiguration<TConfig> config) 
            where TConfig : IDispatcherPoolConfiguration<TConfig>
        {
            return config
               .NrOfInstances(2)
               .WithResizer(new DefaultResizer(2, 10));
        }
    }
}