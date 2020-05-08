namespace Tauron.Host
{
    public interface IApplicationLifetime
    {
        void Shutdown(int exitCode);
    }
}