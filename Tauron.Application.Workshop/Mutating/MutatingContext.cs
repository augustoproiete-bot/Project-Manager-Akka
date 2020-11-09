using System;
using System.Collections.Immutable;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutating
{
    public sealed class MutatingContext<TData>
    {
        public MutatingChange? Change { get; }

        public TData Data { get; }

        public TType GetChange<TType>()
            where TType : MutatingChange
        {
            return Change?.Cast<TType>() ?? throw new InvalidCastException("Change has not the Requested Type");
        }

        private MutatingContext(MutatingChange? change, TData data)
        {
            Change = change;
            Data = data;
        }

        public static MutatingContext<TData> New(TData data)
            => new MutatingContext<TData>(null, data);


        public void Deconstruct(out MutatingChange? mutatingChange, out TData data)
        {
            mutatingChange = Change;
            data = Data;
        }

        public MutatingContext<TData> Update(MutatingChange change, TData newData)
        {
            if (change != null && change != Change && newData is ICanApplyChange<TData> apply)
                newData = apply.Apply(change);

            if (Change != null && change != null && !ReferenceEquals(Change, change))
            {
                if (Change is MultiChange multiChange)
                    change = new MultiChange(multiChange.Changes.Add(change));
                else
                    change = new MultiChange(ImmutableList<MutatingChange>.Empty.Add(change));
            }

            return new MutatingContext<TData>(change, newData);
        }

        public MutatingContext<TData> WithChange(MutatingChange mutatingChange) 
            => Update(mutatingChange, Data);
    }
}