using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public class RepositoryProjectorMap<TProjection, TIdentity> 
        where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
    {
        private readonly IProjectionRepository _repository;

        protected internal readonly ProjectorMap<TProjection, TIdentity, ProjectionContext> ProjectorMap;

        public RepositoryProjectorMap(IProjectionRepository repository)
        {
            _repository = repository;
            ProjectorMap = new ProjectorMap<TProjection, TIdentity, ProjectionContext>
            {
                Create = Create,
                Delete = Delete,
                Update = Update,
                Custom = Custom
            };
        }

        protected virtual Task Custom(ProjectionContext context, Func<Task> projector) 
            => projector();

        protected virtual async Task Update(TIdentity key, ProjectionContext context, Func<TProjection, Task> projector, Func<bool> createifmissing)
        {
            var data = await _repository.Get<TProjection, TIdentity>(context, key);
            if (data == null)
            {
                if (createifmissing())
                    data = await _repository.Create<TProjection, TIdentity>(context, key, p => true);
                else
                    throw new KeyNotFoundException($"The key {key} is not in The Repository");
            }

            await projector(data);
            await _repository.Commit(context, key);
        }

        protected virtual Task<bool> Delete(TIdentity key, ProjectionContext context) 
            => _repository.Delete(context, key);

        protected virtual async Task Create(TIdentity key, ProjectionContext context, Func<TProjection, Task> projector, Func<TProjection, bool> shouldoverwite)
        {
            await projector(await _repository.Create(context, key, shouldoverwite));
            await _repository.Commit(context, key);
        }
    }
}

//Update = async (key, context, pro, missing) =>
//{
//if (!store.TryGetValue(key, out var projection))
//{
//    if (!missing())
//        return;

//    projection = store.GetOrAdd(key, id => new TProjector { Id = id });
//}

//await pro(projection);
//},

//Custom = (context, pro) => pro()