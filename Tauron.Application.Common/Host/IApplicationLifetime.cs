using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IApplicationLifetime
    {
        void Shutdown(int exitCode);
    }
}