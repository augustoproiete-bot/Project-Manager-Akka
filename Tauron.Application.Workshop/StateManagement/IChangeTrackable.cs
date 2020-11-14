using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public interface IChangeTrackable
    {
        bool IsChanged { get; }
    }
}