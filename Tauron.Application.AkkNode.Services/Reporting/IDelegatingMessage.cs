using Akka.Actor;

namespace Tauron.Application.AkkNode.Services
{
    public interface IDelegatingMessage
    {
        Reporter Reporter { get; }

        string Info { get; }

    }
}