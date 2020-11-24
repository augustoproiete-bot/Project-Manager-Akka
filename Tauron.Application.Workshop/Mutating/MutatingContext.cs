using System;
using System.Collections.Immutable;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutating
{
    [PublicAPI]
    public sealed class MutatingContext<TData>
    {
        public Maybe<MutatingChange> Change { get; }

        public TData Data { get; }

        public TType GetChange<TType>()
            where TType : MutatingChange
        {
            var casted =
                from c in Change
                select Maybe.NotNull(c as TType);

            return casted
               .Collapse()
               .OrElse(() => new InvalidCastException("Change has not the Requested Type"));
        }

        private MutatingContext(Maybe<MutatingChange> change, TData data)
        {
            Change = change;
            Data = data;
        }

        public static Maybe<MutatingContext<TData>> New(Maybe<TData> data)
        {
            return
                from d in data
                select new MutatingContext<TData>(Maybe<MutatingChange>.Nothing, d);
        }


        public void Deconstruct(out Maybe<MutatingChange> mutatingChange, out TData data)
        {
            mutatingChange = Change;
            data = Data;
        }

        public MutatingContext<TData> Update(MutatingChange newChange, TData newData) 
            => Update(newChange.ToMaybe(), newData);

        public MutatingContext<TData> Update(Maybe<MutatingChange> mayNewChange, TData newData)
        {
            if (newData is ICanApplyChange<TData> apply)
            {
                var mayapply =
                    from change in mayNewChange
                    where !change.Equals(Change.OrElseDefault())
                    select apply.Apply(change);

                newData = mayapply.Or(newData).Value;
            }
            
            if (Change.IsSomething())
            {
                var changePair =
                    from newChange in mayNewChange
                    from oldChange in Change
                    select (newChange, oldChange);

                changePair.Do(p =>
                {
                    var (oldChnage, newChange) = p;

                    if (ReferenceEquals(oldChnage, newChange)) return;

                    mayNewChange = 
                        (
                        oldChnage is MultiChange multiChange 
                        ? new MultiChange(multiChange.Changes.Add(newChange)) 
                        : new MultiChange(ImmutableList<MutatingChange>.Empty.Add(newChange))
                        ).ToMaybe<MutatingChange>();
                });
            }
            else
                return new MutatingContext<TData>(mayNewChange, newData);

            return new MutatingContext<TData>(mayNewChange, newData);
        }

        public MutatingContext<TData> WithChange<TChange>(Maybe<TChange> mutatingChange)
            where TChange : MutatingChange
            => Update(mutatingChange.Cast<TChange, MutatingChange>(), Data);
        
        public MutatingContext<TData> WithChange<TChange>(TChange mutatingChange)
            where TChange : MutatingChange
            => WithChange(mutatingChange.ToMaybe());
    }
}