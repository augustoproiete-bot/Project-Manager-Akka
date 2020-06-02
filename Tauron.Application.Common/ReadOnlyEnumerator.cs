using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public class ReadOnlyEnumerator<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;


        public ReadOnlyEnumerator([NotNull] IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}