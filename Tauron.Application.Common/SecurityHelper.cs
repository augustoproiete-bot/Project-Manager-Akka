using System.Security;
using JetBrains.Annotations;
using Optional;

namespace Tauron
{
    [PublicAPI]
    public static class SecurityHelper
    {
        public static Option<bool> IsGranted(this Option<IPermission> permission)
        {
            try
            {
                return permission.Map(p =>
                                      {
                                          p.Demand();
                                          return true;
                                      }).Or(false);
            }
            catch (SecurityException)
            {
                return false.Some();
            }
        }
    }
}