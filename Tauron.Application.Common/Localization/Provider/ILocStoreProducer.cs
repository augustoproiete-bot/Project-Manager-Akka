using Akka.Actor;

namespace Tauron.Localization.Provider
{
    public interface ILocStoreProducer
    {
        string Name { get; }

        Props GetProps();
    }
}