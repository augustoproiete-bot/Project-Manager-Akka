using System;
using System.Threading.Tasks;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    [PublicAPI]
    public sealed class ContextDataSource<TData> : IExtendedDataSource<MutatingContext<TData>>, IDisposable
        where TData : class, IStateEntity
    {
        private readonly IExtendedDataSource<TData> _original;

        public ContextDataSource(IExtendedDataSource<TData> original) => _original = original;

        public async Task<Maybe<MutatingContext<TData>>> GetData(IQuery query) 
            => MutatingContext<TData>.New(await _original.GetData(query));

        public async Task SetData(IQuery query, Maybe<MutatingContext<TData>> mayData)
        {
            var maySet =
                from dataContext in mayData
                let data = dataContext.Data
                where data is not IChangeTrackable trackable || trackable.IsChanged
                select data;

            if (maySet.IsSomething())
                await _original.SetData(query, maySet);
        }

        public Task OnCompled(IQuery query) => _original.OnCompled(query);

        public void Dispose()
        {
            if(_original is IDisposable source)
                source.Dispose();
        }
    }
}