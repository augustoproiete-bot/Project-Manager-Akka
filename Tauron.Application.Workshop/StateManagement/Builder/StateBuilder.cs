using System;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public abstract class StateBuilderBase
    {
        public abstract IState Construct();
    }

    public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData>
    {
        private readonly Func<IDataSource<TData>> _source;

        public StateBuilder(Func<IDataSource<TData>> source) 
            => _source = source;
    }
}