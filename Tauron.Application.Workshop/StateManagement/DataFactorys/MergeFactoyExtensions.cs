using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    public partial class MergeFactory
    {
        public static AdvancedDataSourceFactory Merge(params Maybe<AdvancedDataSourceFactory>[] factories) 
            => Merge(factories.AsEnumerable());

        public static AdvancedDataSourceFactory Merge(IEnumerable<Maybe<AdvancedDataSourceFactory>> factorys)
        {
            var target = new MergeFactory();

            void Apply(Maybe<AdvancedDataSourceFactory> mayFactory)
            {
                Do(from factory in mayFactory 
                   select Action(() => target.Register(factory)));
            }

            foreach (var mayFactory in factorys) 
                Apply(mayFactory);

            return target;
        }
        
        public static AdvancedDataSourceFactory Merge(IEnumerable<AdvancedDataSourceFactory> factorys) 
            => Merge(factorys.Select(f => f.ToMaybe()));
    }
}