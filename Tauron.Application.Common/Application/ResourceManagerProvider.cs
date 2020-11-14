using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public static class ResourceManagerProvider
    {
        private static readonly Dictionary<Assembly, ResourceManager> Resources = new();

        public static void Register(ResourceManager manager, Assembly key)
        {
            lock (Resources)
                Resources[Argument.NotNull(key, nameof(key))] = Argument.NotNull(manager, nameof(manager));
        }

        public static void Remove(Assembly key)
        {
            lock(Resources)
                Resources.Remove(key);
        }

        public static Maybe<string> FindResource(string name, Maybe<Assembly> mayKey, Maybe<bool> maySearchEverywere = default)
        {
            lock (Resources)
            {
                var fastTry =
                (
                    from key in mayKey
                    from rm in Resources.Lookup(key)
                    select Maybe.NotNull(rm.GetString(name))
                ).Collapse();

                Maybe<string> SlowTry() =>
                (
                    from searchEverywere in maySearchEverywere.Or(true)
                    where searchEverywere
                    select
                        Maybe.NotNull(
                            (
                                from manager in Resources.Values
                                let res = manager.GetString(name)
                                where !string.IsNullOrWhiteSpace(res)
                                select res
                            ).FirstOrDefault()
                        )
                ).Collapse();

                return fastTry.Or(SlowTry);
            }
        }
    }
}