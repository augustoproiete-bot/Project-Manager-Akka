using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using static Tauron.Preload;

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
                   select Use(() => target.Register(factory)));
            }

            foreach (var mayFactory in factorys) 
                Apply(mayFactory);

            return target;
        }
    }
}