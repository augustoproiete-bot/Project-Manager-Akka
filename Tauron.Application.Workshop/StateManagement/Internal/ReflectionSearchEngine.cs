using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
using static Tauron.Preload;

using TypeLst = System.Collections.Immutable.ImmutableList<System.Type>;
using TypeGroupDic = System.Collections.Immutable.ImmutableDictionary<System.Type, System.Collections.Immutable.ImmutableList<System.Type>>;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public class ReflectionSearchEngine
    {
        private sealed record ReflectionData(ImmutableList<(Type State, Maybe<string> Key)> States, TypeGroupDic Reducers, ImmutableList<Func<Maybe<AdvancedDataSourceFactory>>> Factorys, TypeLst Processors);

        private static readonly MethodInfo ConfigurateStateMethod = typeof(ReflectionSearchEngine).GetMethod(nameof(ConfigurateState), BindingFlags.Instance | BindingFlags.NonPublic)
         ?? throw new InvalidOperationException("Method not Found");

        private readonly Assembly _assembly;
        private readonly Maybe<IComponentContext> _context;

        public ReflectionSearchEngine(Assembly assembly, Maybe<IComponentContext> context)
        {
            _assembly = assembly;
            _context = context;
        }

        public void Add(ManagerBuilder builder, IDataSourceFactory factory)
        {
            var data = new ReflectionData(TypeLst.Empty, TypeGroupDic.Empty, ImmutableList<Func<Maybe<AdvancedDataSourceFactory>>>.Empty, TypeLst.Empty);

            Func<Maybe<TType>> CreateFactory<TType>(Type target)
            {
                Maybe<TType> TryCreate()
                    => from obj in MayNotNull(FastReflection.Shared.FastCreateInstance(target))
                       where obj is TType
                       select (TType) obj;

                Maybe<TType> TryResolve()
                    => from context in _context
                       from obj in MayNotNull(context.ResolveOptional(target))
                       where obj is TType
                       select (TType) obj;

                return () => Or(TryResolve, TryCreate);
            }

            void ApplyAttribute(Type targetType, object attr)
            {
                TypeGroupDic AddToDic(TypeGroupDic dic, Type key) 
                    => dic.ContainsKey(key) 
                        ? dic.SetItem(key, dic[key].Add(targetType)) 
                        : dic.Add(key, TypeLst.Empty.Add(targetType));

                ReflectionData Switch(ReflectionData data)
                {
                    switch (attr)
                    {
                        case StateAttribute state:
                             return data with{ States = data.States.Add((targetType, MayNotEmpty(state.Key))) };
                        case EffectAttribute:
                            builder.WithEffect(CreateFactory<IEffect>(targetType));
                            return data;
                        case MiddlewareAttribute _:
                            builder.WithMiddleware(CreateFactory<IMiddleware>(targetType));
                            return data;
                        case BelogsToStateAttribute belogsTo:
                            return data with{Reducers = data}
                                reducers.Add(belogsTo.StateType, type);
                            break;
                        case DataSourceAttribute _:
                            factorys.Add(CreateFactory<AdvancedDataSourceFactory>(type));
                            break;
                        case ProcessorAttribute _:
                            processors.Add(type);
                            break;
                        default:
                            return data;
                    }
                }
            }

            var types = _assembly.GetTypes();

            foreach (var type in types)
            {
                foreach (var customAttribute in type.GetCustomAttributes(false))
                {
                    ApplyAttribute(customAttribute);
                }
            }

            //if (factorys.Count != 0)
            //{
            //    var factory1 = factory;
            //    factorys.Add(() => factory1.MaybeCast<IDataSourceFactory, AdvancedDataSourceFactory>());
            //    factory = MergeFactory.Merge(factorys.Select(f => f()));
            //}

            //foreach (var (type, key) in states)
            //{
            //    if(type == null || type.BaseType?.IsGenericType != true || type.BaseType?.GetGenericTypeDefinition() != typeof(StateBase<>)) 
            //        continue;

            //    var dataType = type.BaseType.GetGenericArguments()[0];
            //    var actualMethod = ConfigurateStateMethod.MakeGenericMethod(dataType);
            //    actualMethod.Invoke(this, new object?[] {type, builder, factory, reducers, key});
            //}

            //foreach (var processor in processors) 
            //    builder.Superviser.CreateAnonym(processor, $"Processor--{processor.Name}");
        }

        private void ConfigurateState<TData>(Type target, ManagerBuilder builder, IDataSourceFactory factory, GroupDictionary<Type, Type> reducerMap, string? key)
            where TData : class, IStateEntity
        {
            var config = builder.WithDataSource(factory.Create<TData>());

            if (!string.IsNullOrWhiteSpace(key))
                config.WithKey(key);

            config.WithStateType(target);
            if (target.GetCustomAttribute(typeof(CacheAttribute)) is CacheAttribute cache && cache.UseParent)
                config.WithParentCache();

            foreach (var methodInfo in target.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (!(methodInfo.GetCustomAttribute(typeof(CacheAttribute)) is CacheAttribute metCache)) continue;
                
                if (metCache.UseParent)
                    config.WithParentCache();

                config.WithCache((Action<ConfigurationBuilderCachePart>) Delegate.CreateDelegate(typeof(Action<ConfigurationBuilderCachePart>), methodInfo));
            }

            if (!reducerMap.TryGetValue(target, out var reducers)) return;
            
            var methods = new Dictionary<Type, MethodInfo>();
            var validators = new Dictionary<Type, object>();

            foreach (var reducer in reducers)
            {
                foreach (var method in reducer.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if(!method.HasAttribute<ReducerAttribute>())
                        continue;

                    var parms = method.GetParameters();
                    if(parms.Length != 2)
                        continue;
                    if(!parms[0].ParameterType.IsGenericType)
                        continue;
                    if(parms[0].ParameterType.GetGenericTypeDefinition() != typeof(MutatingContext<>))
                        continue;
                    methods[parms[1].ParameterType] = method;
                }

                foreach (var property in reducer.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if(!property.HasAttribute<ValidatorAttribute>())
                        continue;

                    var potenialValidator = property.PropertyType;
                    if(!potenialValidator.IsAssignableTo<IValidator>())
                        continue;

                    var validatorType = potenialValidator.GetInterface(typeof(IValidator<>).Name);
                    if(validatorType == null)
                        continue;
                    var validator = property.GetValue(null);
                    if(validator == null)
                        continue;

                    validators[validatorType.GenericTypeArguments[0]] = validator;
                }
            }

            foreach (var (actionType, reducer) in methods)
            {
                Type? delegateType = null;

                //Sync Version
                var returnType = reducer.ReturnType;
                if (returnType == typeof(MutatingContext<TData>))
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(MutatingContext<TData>));
                else if (returnType == typeof(ReducerResult<TData>))
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(ReducerResult<TData>));
                else if (returnType.IsAssignableTo<TData>())
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(TData));

                //AsyncVersion
                if (returnType == typeof(Task<MutatingContext<TData>>))
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(Task<MutatingContext<TData>>));
                else if (returnType == typeof(Task<ReducerResult<TData>>))
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(Task<ReducerResult<TData>>));
                else if (returnType.IsAssignableTo<Task<TData>>())
                    delegateType = typeof(Func<,,>).MakeGenericType(typeof(MutatingContext<TData>), actionType, typeof(Task<TData>));

                if (delegateType == null)
                    continue;

                var acrualDelegate = Delegate.CreateDelegate(delegateType, reducer);
                object? validator = null;
                if (validators.ContainsKey(actionType))
                    validator = validators[actionType];

                var constructedReducer = typeof(DelegateReducer<,>).MakeGenericType(actionType, typeof(TData));
                var reducerInstance = Activator.CreateInstance(constructedReducer, acrualDelegate, validator);

                config.WithReducer(() => (IReducer<TData>) reducerInstance);
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class DelegateReducer<TAction, TData> : Reducer<TAction, TData> 
            where TData : IStateEntity 
            where TAction : IStateAction
        {
            private readonly Func<MutatingContext<TData>, TAction, Task<ReducerResult<TData>>> _action;

            public DelegateReducer(Func<MutatingContext<TData>, TAction, ReducerResult<TData>> action, IValidator<TAction>? validation)
            {
                _action = (a, c) => Task.FromResult(action(a, c));
                Validator = validation;
            }

            public DelegateReducer(Func<MutatingContext<TData>, TAction, MutatingContext<TData>> action, IValidator<TAction>? validation)
            {
                _action = (context, stateAction) => SucessAsync(action(context, stateAction));
                Validator = validation;
            }

            public DelegateReducer(Func<MutatingContext<TData>, TAction, TData> action, IValidator<TAction>? validation)
            {
                _action = (context, stateAction) => SucessAsync(MutatingContext<TData>.New(action(context, stateAction)));
                Validator = validation;
            }

            public DelegateReducer(Func<MutatingContext<TData>, TAction, Task<ReducerResult<TData>>> action, IValidator<TAction>? validation)
            {
                _action = action;
                Validator = validation;
            }

            public DelegateReducer(Func<MutatingContext<TData>, TAction, Task<MutatingContext<TData>>> action, IValidator<TAction>? validation)
            {
                _action = async (context, stateAction) => Sucess(await action(context, stateAction));
                Validator = validation;
            }

            public DelegateReducer(Func<MutatingContext<TData>, TAction, Task<TData>> action, IValidator<TAction>? validation)
            {
                _action = async (context, stateAction) => Sucess(MutatingContext<TData>.New(await action(context, stateAction)));
                Validator = validation;
            }

            public override IValidator<TAction>? Validator { get; }

            protected override async Task<ReducerResult<TData>> Reduce(MutatingContext<TData> state, TAction action) 
                => await _action(state, action);
        }
    }
}