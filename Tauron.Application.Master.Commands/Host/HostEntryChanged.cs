using Akka.Actor;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class HostEntryChanged
    {
        public string Name { get; }

        public ActorPath Path { get; }

        public bool Removed { get; }

        public HostEntryChanged(string name, ActorPath path, bool removed)
        {
            Name = name;
            Path = path;
            Removed = removed;
        }
    }
}